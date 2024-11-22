﻿#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using S7.Net;
using System.Net.Sockets;
using System.Net;
using static Mrgada;
using System.Diagnostics;
using Opc.UaFx.Client;
using Opc.Ua;


public static partial class Mrgada
{
    public partial class Acquisitor
    {
        // OPCUA Acquisitor
        public Opc.UaFx.Client.OpcClient _OpcUaClient;
        private Func<Opc.UaFx.Client.OpcClient> _OpcUaClientGetter;
        private Thread _OpcUaClientAcquisitorThread;
        public List<OpcUaTag> _OpcUaTags = [];

        public virtual void InitializeOpcUaNodes()
        {
            // Specific tags are added in "./Procedurally Generated/{Acquisitor name}"
        }

        public class OpcUaTag
        {
            private string _NodeId;
            private float _cv;
            private Func<Opc.UaFx.Client.OpcClient> _OpcUaClientGetter;

            public float CV
            {
                get => _cv;
                set
                {
                    WriteNode(_cv);
                }
            }

            public OpcUaTag(string NodeId, Func<Opc.UaFx.Client.OpcClient> OpcUaClientGetter)
            {
                _NodeId = NodeId;
                _OpcUaClientGetter = OpcUaClientGetter;
            }

            public void ReadNode()
            {
                var _OpcUaClient = _OpcUaClientGetter();
                var value = _OpcUaClient.ReadNode(_NodeId).Value;
                _cv = Convert.ToSingle(value);
            }

            private void WriteNode(float value)
            {
                var _OpcUaClient = _OpcUaClientGetter();
                _OpcUaClient.WriteNode(_NodeId, value);
            }
        }


        private void InitializeOPCUAAcquisitorHandlerThread()
        {
            InitializeOpcUaNodes();

            _OpcUaClientAcquisitorThread = new Thread(new ThreadStart(async () =>
            {
                Stopwatch ConsoleWriteWatch = new Stopwatch();
                ConsoleWriteWatch.Start();

                while (true)
                {
                    if (IsConnected)
                    {
                        bool ConsoleWrite = ConsoleWriteWatch.ElapsedMilliseconds >= 5000;
                        if (ConsoleWrite) ConsoleWriteWatch.Restart();

                        Thread.Sleep(_AcquisitorThreadSleep);

                        Stopwatch Stopwatch = new Stopwatch();

                        // Read bytes from PLC
                        Stopwatch.Start();
                        foreach (OpcUaTag Tag in _OpcUaTags)
                        {
                            Tag.ReadNode();
                        }
                        Stopwatch.Stop();
                        if (ConsoleWrite) Console.WriteLine($"{_AcquisitorName} Reading tags from OPCUA Server took: {Stopwatch.ElapsedMilliseconds} ms");
                    }
                    else
                    {
                        try
                        {
                            _OpcUaClient = new Opc.UaFx.Client.OpcClient($"opc.tcp://{_AcquisitorIp}:4840");
                            _OpcUaClient.Connect();
                            IsConnected = true;
                        }
                        catch
                        {
                            IsConnected = false;
                            Console.WriteLine($"{_AcquisitorName} Can't connect to OPCUA Server, trying again in 30 seconds");
                            Thread.Sleep(30000);
                        }
                    }
                }
            }));
            _OpcUaClientAcquisitorThread.IsBackground = true; // Doesn't block application exit
            _OpcUaClientAcquisitorThread.Start();
        }
    }
}
