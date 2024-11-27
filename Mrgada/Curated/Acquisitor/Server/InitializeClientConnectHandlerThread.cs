#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using S7.Net;
using System.Net.Sockets;
using System.Net;
using static Mrgada;
using System.Security.Cryptography.X509Certificates;
using Serilog;

public static partial class Mrgada
{
    public partial class Acquisitor 
    {  
        private Thread _ClientHandlerThread;
        private object _ClientHandlerThreadLock = new();
        private List<TcpClient> _Clients = new();

        private Thread _ClientMonitorThread;

        public virtual void OnClientConnect(TcpClient client)
        {
            // Override this method to handle client connections
        }
        private void InitializeClientConnectHandlerThread()
        {
            // Start the thread to accept incoming clients
            _ClientHandlerThread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    try
                    {
                        TcpClient client = _AcquisitorTcpListener.AcceptTcpClient();
                        lock (_ClientHandlerThreadLock)
                        {
                            _Clients.Add(client);
                        }


                        Log.Information($"Client {_Clients.Count} Connected to Acquisitor {_AcquisitorName}!");
                        OnClientConnect(client);
                    }

                    catch (SocketException)
                    {
                        // This exception is expected when the server stops, so we just break the loop.
                        break;
                    }
                }
            }));
            _ClientHandlerThread.IsBackground = true; // Doesn't block application exit
            _ClientHandlerThread.Start();

            // Start the thread to monitor client connections
            StartClientMonitorThread();
        }

        private void StartClientMonitorThread()
        {
            _ClientMonitorThread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    lock (_ClientHandlerThreadLock)
                    {
                        // Iterate over the clients and check if any are disconnected
                        for (int i = _Clients.Count - 1; i >= 0; i--)
                        {
                            TcpClient client = _Clients[i];

                            if (!IsClientConnected(client))
                            {
                                Log.Information($"Client disconnected from Acquisitor {_AcquisitorName}");
                                // Remove the client from the list
                                _Clients.RemoveAt(i);
                                // Optionally, close the client to free resources
                                client.Close();
                            }
                        }
                    }
                    // Sleep for a short duration to reduce CPU usage
                    Thread.Sleep(1000);
                }
            }));
            _ClientMonitorThread.IsBackground = true; // Doesn't block application exit
            _ClientMonitorThread.Start();
        }

        private bool IsClientConnected(TcpClient client)
        {
            try
            {
                // Check if the client is connected by checking the state of the socket
                return !(client.Client.Poll(1, SelectMode.SelectRead) && client.Client.Available == 0);
            }
            catch (SocketException)
            {
                // If there's a socket exception, the client is definitely not connected
                return false;
            }
        }
    }
}
