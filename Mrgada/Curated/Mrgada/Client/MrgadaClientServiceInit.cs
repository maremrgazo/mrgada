using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public static partial class Mrgada
{
    private static CancellationTokenSource cancellationTokenSource;
    private static Thread _MrgadaClientConnectThread;
    private static bool _MrgadatClientConnected = false;
    private static TcpClient _MrgadaTcpClient;
    private static NetworkStream _MrgadaClientNetworkStream;
    private static Thread _MrgadaClientBroadcastListenThread;

    public static void MrgadaClientServiceInit()
    {
        InitializeClientConnectThread();
    }
    static void InitializeClientConnectThread()
    {
        _MrgadaClientConnectThread = new Thread(new ThreadStart(() =>
        {
            while (true)
            {
                try
                {
                    if (!_MrgadatClientConnected)
                    {
                        _MrgadaTcpClient = new TcpClient();
                        _MrgadaTcpClient.Connect(Mrgada._ServerIp, Mrgada._MrgadaServerPort);
                        Console.WriteLine($"Client has connected to Mrgada Server!");
                        _MrgadaClientNetworkStream = _MrgadaTcpClient.GetStream();
                        _MrgadatClientConnected = true;

                        // Initialize a new cancellation token source for each connection
                        cancellationTokenSource = new CancellationTokenSource();
                        // Start the ClientBroadcastListenThread when connected
                        StartClientBroadcastListenThread(cancellationTokenSource.Token);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Connection attempt failed: " + ex.Message);

                    Console.WriteLine("Retrying connection in 3 seconds...");
                    Thread.Sleep(3000);
                }
            }
        }));

        // Set the thread to be a background thread
        _MrgadaClientConnectThread.IsBackground = true;
        _MrgadaClientConnectThread.Start();
    }

    static void StartClientBroadcastListenThread(CancellationToken cancellationToken)
    {
        _MrgadaClientBroadcastListenThread = new Thread(() =>
        {
            byte[] BroadcastBuffer = new byte[65535];

            try
            {
                while (_MrgadatClientConnected && !cancellationToken.IsCancellationRequested)
                {
                    if (_MrgadaClientNetworkStream.CanRead)
                    {
                        int bytesRead = _MrgadaClientNetworkStream.Read(BroadcastBuffer, 0, BroadcastBuffer.Length);
                        if (bytesRead == 0)
                        {
                            Console.WriteLine($"Mrgada Server has closed the connection.");
                            _MrgadatClientConnected = false;
                            break;
                        }

                        OnBroadcastRecieved(BroadcastBuffer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection lost to Mrgada Server:" + ex.Message);
                _MrgadatClientConnected = false;
            }
            finally
            {
                // Clean up and reset connection state
                Disconnect();
            }
        });
        _MrgadaClientBroadcastListenThread.IsBackground = true;
        _MrgadaClientBroadcastListenThread.Start();
    }

    static void Disconnect()
    {
        if (_MrgadaTcpClient != null)
        {
            try
            {
                if (_MrgadaClientNetworkStream != null)
                {
                    _MrgadaClientNetworkStream.Close();
                    _MrgadaClientNetworkStream.Dispose();
                }

                _MrgadaTcpClient.Close();
                _MrgadaTcpClient.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during disconnect from Mrgada Server: " + ex.Message);
            }
            finally
            {
                Console.WriteLine($"Disconnected from Mrgada Server.");
                _MrgadatClientConnected = false;

                // Cancel the listening thread gracefully
                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource.Dispose();
                }

                if (_MrgadaClientBroadcastListenThread != null && _MrgadaClientBroadcastListenThread.IsAlive)
                {
                    _MrgadaClientBroadcastListenThread.Join(); // Wait for the thread to exit
                }
            }
        }
    }


}
