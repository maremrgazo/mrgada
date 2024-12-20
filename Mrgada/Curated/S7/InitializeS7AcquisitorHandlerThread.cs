﻿#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using S7.Net;
using System.Net.Sockets;
using System.Net;
using static Mrgada;
using System.Diagnostics;
using Serilog;
using SerilogTimings;




public static partial class Mrgada
{
    public partial class Acquisitor
    {
        private int _DebugInterval = 5000;
        private bool _ConsoleWrite = false;
        public object _S7ByteLock = new();
        public class S7db
        {
            public readonly int Num;
            public readonly int Len;
            public byte[] Bytes;
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
                lock (_Acquisitor._S7ByteLock)
                {
                    Bytes = _S7Plc.ReadBytes(S7.Net.DataType.DataBlock, this.Num, 0, this.Len);
                    //if (!BytesOld.AsSpan().SequenceEqual(Bytes))
                    //{
                    //    _Acquisitor.AcquisitorBroadcast(Bytes);
                    //}
                    if (!Bytes.SequenceEqual(BytesOld))
                    {
                        short dbNum = (short)this.Num;
                        byte[] dbNumByteArray = BitConverter.GetBytes(dbNum);

                        short BroadcastBytesLength = (short)(dbNumByteArray.Length + Bytes.Length + 2);
                        byte[] BroadcastBytesLengthByteArray = BitConverter.GetBytes(BroadcastBytesLength);

                        _Acquisitor.AcquisitorBroadcastBytes.AddRange(BroadcastBytesLengthByteArray);
                        _Acquisitor.AcquisitorBroadcastBytes.AddRange(dbNumByteArray);
                        _Acquisitor.AcquisitorBroadcastBytes.AddRange(Bytes);
                    }
                    BytesOld = Bytes;
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
        private List<Byte> AcquisitorBroadcastBytes = [];
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
                        Stopwatch iterationTimer = Stopwatch.StartNew();

                        _ConsoleWrite = ConsoleWriteWatch.ElapsedMilliseconds >= _DebugInterval;
                        if (_ConsoleWrite) ConsoleWriteWatch.Restart();

                        Thread.Sleep(_AcquisitorThreadInterval);

                        // Parse CVs
                        //void ParseCVs()
                        //{
                        //    foreach (S7db db in _S7dbs)
                        //    {
                        //        db.ParseCVs();
                        //    }
                        //}

                        if (_ConsoleWrite)
                        {
                            using (Operation.Time($"{_AcquisitorName,-10}: {"Reading and Parsing bytes from S7 PLC",-50}"))
                            {
                                ReadS7dbs();
                                //ParseCVs();
                            }
                        }
                        else
                        {
                            ReadS7dbs();
                            //ParseCVs();
                        }
                        //ReadS7dbs();

                        //// Broadcast CVs to Clients
                        AcquisitorServerBroadcast(AcquisitorBroadcastBytes);
                        AcquisitorBroadcastBytes = [];

                        Thread.Sleep(_AcquisitorThreadInterval);

                       

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
                            _S7Plc.Open();
                            IsConnected = true;
                        }
                        catch
                        {
                            IsConnected = false;
                            Log.Information($"{_AcquisitorName} Can't connect to S7 PLC, trying again in 30 seconds");
                            await Task.Delay(30000);
                        }
                    }
                }
            }));
            _S7AcquisitorThread.IsBackground = true; // Doesn't block application exit
            _S7AcquisitorThread.Start();
        }
    }
}
