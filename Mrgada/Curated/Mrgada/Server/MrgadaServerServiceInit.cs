#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public static partial class Mrgada
{
    private static TcpListener _MrgadaTcpListener;
    private static List<TcpClient> _MrgadaClients = [];
    private static object _MrgadaClientHandlerThreadLock = new();
    private static Thread _MrgadaClientHandlerThread;
    private static Thread _MrgadaClientMonitorThread;
    public static void MrgadaServerServiceInit()
    {
        IPAddress MrgadaServerIp = IPAddress.Parse(Mrgada._ServerIp);
        _MrgadaTcpListener = new TcpListener(MrgadaServerIp, Mrgada._MrgadaServerPort);
        _MrgadaTcpListener.Start();
        Console.WriteLine($"Mrgada TCP Server Started!");

        InitializeClientConnectHandlerThread();
    }
    static void InitializeClientConnectHandlerThread()
    {

        // Start the thread to accept incoming clients
        _MrgadaClientHandlerThread = new Thread(new ThreadStart(() =>
        {
            while (true)
            {
                try
                {
                    TcpClient client = _MrgadaTcpListener.AcceptTcpClient();
                    lock (_MrgadaClientHandlerThreadLock)
                    {
                        _MrgadaClients.Add(client);
                    }


                    Console.WriteLine($"Client {_MrgadaClients.Count} Connected to Mrgada Server!");
                }

                catch (SocketException)
                {
                    // This exception is expected when the server stops, so we just break the loop.
                    break;
                }
            }
        }));
        _MrgadaClientHandlerThread.IsBackground = true; // Doesn't block application exit
        _MrgadaClientHandlerThread.Start();

        // Start the thread to monitor client connections
        MrgadaStartClientMonitorThread();
    }
    static void MrgadaStartClientMonitorThread()
    {
        _MrgadaClientMonitorThread = new Thread(new ThreadStart(() =>
        {
            while (true)
            {
                lock (_MrgadaClientHandlerThreadLock)
                {
                    // Iterate over the clients and check if any are disconnected
                    for (int i = _MrgadaClients.Count - 1; i >= 0; i--)
                    {
                        TcpClient client = _MrgadaClients[i];

                        if (!IsClientConnected(client))
                        {
                            Console.WriteLine($"Client disconnected from Mrgada Server");
                            // Remove the client from the list
                            _MrgadaClients.RemoveAt(i);
                            // Optionally, close the client to free resources
                            client.Close();
                        }
                    }
                }
                // Sleep for a short duration to reduce CPU usage
                Thread.Sleep(1000);
            }
        }));
        _MrgadaClientMonitorThread.IsBackground = true; // Doesn't block application exit
        _MrgadaClientMonitorThread.Start();
    }

    static bool IsClientConnected(TcpClient client)
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
