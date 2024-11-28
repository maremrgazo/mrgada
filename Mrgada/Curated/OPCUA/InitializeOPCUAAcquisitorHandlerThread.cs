#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using S7.Net;
using System.Net.Sockets;
using System.Net;
using static Mrgada;
using System.Diagnostics;
using Opc.UaFx.Client;
using Opc.Ua;
using static Mrgada.Acquisitor;
using Serilog;
using SerilogTimings;


public static partial class Mrgada
{
    public partial class Acquisitor
    {
        // OPCUA Acquisitor
        public Opc.UaFx.Client.OpcClient _OpcUaClient;
        private Thread _OpcUaClientAcquisitorThread;
        public List<Object> _OpcUaTags = [];

        public virtual void InitializeOpcUaNodes()
        {
            // Specific tags are added in "./Procedurally Generated/{Acquisitor name}"
        }

        public class OpcUaTag<T>
        {
            private string _NodeId;
            private T _cv;
            private Func<Opc.UaFx.Client.OpcClient> _OpcUaClientGetter;

            public T CV
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
                _cv = (T)_OpcUaClient.ReadNode(_NodeId).Value;
            }
            private void WriteNode(T value)
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
                        Stopwatch iterationTimer = Stopwatch.StartNew();

                        _ConsoleWrite = ConsoleWriteWatch.ElapsedMilliseconds >= _DebugInterval;
                        if (_ConsoleWrite) ConsoleWriteWatch.Restart();

                        void ReadOpcTags()
                        {
                            var floatNodes = _OpcUaTags.OfType<OpcUaTag<float>>();
                            var intSingle = _OpcUaTags.OfType<OpcUaTag<System.Int16>>();

                            foreach (var NodeFloat in floatNodes) { NodeFloat.ReadNode(); }
                            foreach (var NodeSingle in intSingle) { NodeSingle.ReadNode(); }
                        }

                        if (_ConsoleWrite)
                        {
                            using (Operation.Time($"{_AcquisitorName,-10}: {"Reading tags from OPCUA Server", -50}"))
                            {
                                ReadOpcTags();
                            }
                        }
                        else
                        {
                            ReadOpcTags();
                        }

                        iterationTimer.Stop();
                        int remainingTime = (int)(_AcquisitorThreadInterval - iterationTimer.ElapsedMilliseconds);
                        if (remainingTime > 0)
                        {
                            Thread.Sleep(remainingTime);
                        }
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
                            Console.WriteLine($"{_AcquisitorName,-10}: Can't connect to OPCUA Server, trying again in 30 seconds");
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
