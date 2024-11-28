#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using S7.Net;
using System.Net.Sockets;
using System.Net;
using Serilog;
using System.Collections;
using static Mrgada.MRP6;
using System.Security.Cryptography.X509Certificates;

public static partial class Mrgada
{
    public partial class Acquisitor
    {
        private CancellationTokenSource cancellationTokenSource;
        private Thread ClientConnectThread;
        private bool ClientConnected = false;
        private TcpClient TcpClient;
        private NetworkStream ClientNetworkStream;
        private Thread ClientBroadcastListenThread;
        public void InitializeClientConnectThread() 
        {
            ClientConnectThread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    try
                    {
                        if (!ClientConnected)
                        {
                            TcpClient = new TcpClient();
                            TcpClient.Connect(Mrgada._ServerIp, _AcquisitorTcpPort);
                            Log.Information($"Client has connected to {_AcquisitorName} Acquisitor!");
                            ClientNetworkStream = TcpClient.GetStream();
                            ClientConnected = true;

                            // Initialize a new cancellation token source for each connection
                            cancellationTokenSource = new CancellationTokenSource();
                            // Start the ClientBroadcastListenThread when connected
                            StartClientBroadcastListenThread(cancellationTokenSource.Token);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Information($"Connection attempt to Mrgada {Mrgada._ServerIp} failed: " + ex.Message);

                        Log.Information("Retrying connection in 3 seconds...");
                        Thread.Sleep(3000);
                    }
                }
            }));

            // Set the thread to be a background thread
            ClientConnectThread.IsBackground = true;
            ClientConnectThread.Start();
        }

        public virtual void ParseAcquisitorBroadcast(byte[] Broadcast)
        {
        }

        private void StartClientBroadcastListenThread(CancellationToken cancellationToken)
        {
            ClientBroadcastListenThread = new Thread(() =>
            {
                byte[] BroadcastBuffer = new byte[65563];

                try
                {
                    while (ClientConnected && !cancellationToken.IsCancellationRequested)
                    {
                        if (ClientNetworkStream.CanRead)
                        {
                            int bytesRead = ClientNetworkStream.Read(BroadcastBuffer, 0, BroadcastBuffer.Length);
                            if (bytesRead == 0)
                            {
                                Log.Information($"{_AcquisitorName} Acquisitor has closed the connection.");
                               ClientConnected = false;
                                break;
                            }

                            ParseAcquisitorBroadcast(BroadcastBuffer);


                            Log.Information($"{_AcquisitorName} Acquisitor has Sent Broadcast:");
                            for (int j = 0; j < 10; j++) { Console.Write(BroadcastBuffer[j] + " "); }
                            //Log.Information();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Information($"Connection lost to Acquisitor {_AcquisitorName}: " + ex.Message);
                    ClientConnected = false;
                }
                finally
                {
                    // Clean up and reset connection state
                    Disconnect();
                }
            });
            ClientBroadcastListenThread.IsBackground = true;
            ClientBroadcastListenThread.Start();
        }

        private void Disconnect()
        {
            if (TcpClient != null)
            {
                try
                {
                    if (ClientNetworkStream != null)
                    {
                        ClientNetworkStream.Close();
                        ClientNetworkStream.Dispose();
                    }

                    TcpClient.Close();
                    TcpClient.Dispose();
                }
                catch (Exception ex)
                {
                    Log.Information($"Error during disconnect from Acquisitor {_AcquisitorName}: " + ex.Message);
                }
                finally
                {
                    Log.Information($"Disconnected from Acquisitor {_AcquisitorName}.");
                    ClientConnected = false;

                    // Cancel the listening thread gracefully
                    if (cancellationTokenSource != null)
                    {
                        cancellationTokenSource.Cancel();
                        cancellationTokenSource.Dispose();
                    }

                    if (ClientBroadcastListenThread != null && ClientBroadcastListenThread.IsAlive)
                    {
                        ClientBroadcastListenThread.Join(); // Wait for the thread to exit
                    }
                }
            }
        }


    }

}
