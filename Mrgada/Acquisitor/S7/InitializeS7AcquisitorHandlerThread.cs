﻿#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using S7.Net;
using System.Net.Sockets;
using System.Net;
using static Mrgada;
using System.Diagnostics;


public static partial class Mrgada
{
    public partial class Acquisitor
    {
        private int _DebugInterval = 5000;
        private bool _ConsoleWrite = false;

        private void AcquisitorBroadcast(byte[] Bytes)
        {
            if (_Clients.Count > 0)
            {
                foreach (TcpClient Client in _Clients)
                {
                    try
                    {
                        NetworkStream Stream = Client.GetStream();
                        Stream.Write(Bytes, 0, Bytes.Length);
                    }
                    catch (Exception)
                    {
                        // Handle any exceptions (e.g., client disconnected during broadcast)
                        if (_ConsoleWrite) Console.WriteLine($"Error while Acquisitor {_AcquisitorName} was broadcasting bytes!");
                        break;
                    }
                }
                if (_ConsoleWrite) Console.WriteLine($"Acquisitor {_AcquisitorName}Broadcast following bytes to {_Clients.Count} clients: ");
                for (int i = 0; i < (Bytes.Length > 10 ? 10 : Bytes.Length); i++)
                {
                    if (_ConsoleWrite) Console.Write(Bytes[i] + " ");
                }
                if (_ConsoleWrite) Console.WriteLine();
            }
            else
            {
                if (_ConsoleWrite) Console.WriteLine($"No Clients connected, Acquisitor {_AcquisitorName} didn't broadcast any bytes");
            }
        }

        public class S7db
        {
            public readonly int Num;
            public readonly int Len;
            private byte[] Bytes;
            private byte[] BytesOld;
            private S7.Net.Plc _S7Plc;
            private Acquisitor _Acquisitor;

            public S7db(int Num, int Len, S7.Net.Plc _S7Plc, Acquisitor _Acquisitor)
            {
                this.Num = Num;
                this.Len = Len;
                this._S7Plc = _S7Plc;
                this._Acquisitor = _Acquisitor;

                Bytes = new byte[Len];
                BytesOld = new byte[Len];
            }

            public void Read()
            {
                BytesOld = Bytes;
                Bytes = _S7Plc.ReadBytes(S7.Net.DataType.DataBlock, Num, 0, Len);

                if (!BytesOld.AsSpan().SequenceEqual(Bytes))
                {
                    _Acquisitor.AcquisitorBroadcast(Bytes);
                }
            }

            public void OnClientConnect()
            {
                BytesOld = new byte[Len];
            }

            public virtual void ParseCVs()
            {
            }
        }

        public virtual void InitializeS7dbs()
        {
            // Add specific dbs like dbDigitalValves, etc...

            //// For testing purposes
            //for (int i = 0; i < 100; i++) 
            //    _S7dbs.Add(new S7db() { Num = 52, Len = 200 });
        }

        public virtual void ReadS7dbs()
        {
            // Add specific dbs like dbDigitalValves, etc...
        }

        // S7 Acquisitor
        public S7.Net.Plc _S7Plc;
        private Thread _S7AcquisitorThread;
        private List<S7db> _S7dbs = [];
        private void InitializeS7AcquisitorHandlerThread()
        {
            _S7Plc = new S7.Net.Plc((S7.Net.CpuType)_AcquisitorType, _AcquisitorIp, 0, 1); // TODO Add Rack and Slot for S7 Acquisitors

            InitializeS7dbs();

            _S7AcquisitorThread = new Thread(new ThreadStart(async () =>
            {
                Stopwatch ConsoleWriteWatch = new Stopwatch();
                ConsoleWriteWatch.Start();

                while (true)
                {
                    if (_S7Plc.IsConnected)
                    {
                        _ConsoleWrite = ConsoleWriteWatch.ElapsedMilliseconds >= _DebugInterval;
                        if (_ConsoleWrite) ConsoleWriteWatch.Restart();

                        Thread.Sleep(_AcquisitorThreadSleep);

                        Stopwatch Stopwatch = new Stopwatch();

                        // Read bytes from PLC
                        Stopwatch.Start();
                        //foreach (S7db db in _S7dbs)
                        //{
                        //    db.bytes = await _S7Plc.ReadBytesAsync(S7.Net.DataType.DataBlock, db.Num, 0, db.Len); // TODO Implement Async
                        //}
                        ReadS7dbs();
                        Stopwatch.Stop();
                        if (_ConsoleWrite) Console.WriteLine($"{_AcquisitorName} Reading bytes from S7PLC took: {Stopwatch.ElapsedMilliseconds} ms");

                        // Parse CVs
                        Stopwatch.Restart();
                        foreach (S7db db in _S7dbs)
                        {
                            db.ParseCVs(); // TODO Implement Async
                        }
                        Stopwatch.Stop();
                        if (_ConsoleWrite) Console.WriteLine($"{_AcquisitorName} Parsing CVs from S7 PLC took: {Stopwatch.ElapsedMilliseconds} ms");

                        // Broadcast CVs to Clients
                        //MrgadaServerBroadcast(BroadcastBytes);

                    }
                    else
                    {
                        try
                        {
                            _S7Plc.Open();
                            IsConnected = true;
                        }
                        catch
                        {
                            IsConnected = false;
                            Console.WriteLine($"{_AcquisitorName} Can't connect to S7 PLC, trying again in 30 seconds");
                            Thread.Sleep(30000);
                        }
                    }
                }
            }));
            _S7AcquisitorThread.IsBackground = true; // Doesn't block application exit
            _S7AcquisitorThread.Start();
        }
    }
}
