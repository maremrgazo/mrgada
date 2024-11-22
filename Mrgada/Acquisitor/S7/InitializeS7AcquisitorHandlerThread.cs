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
        public class S7db
        {
            public int Num;
            public int Len;
            public byte[] bytes;

            public virtual void ParseCVs()
            {
            }
        }

        public virtual void InitializeS7dbs()
        {
            // Add specific dbs like dbDigitalValves, etc...

            // For testing purposes
            for (int i = 0; i < 100; i++) 
                _S7dbs.Add(new S7db() { Num = 52, Len = 200 });
        }


        // S7 Acquisitor
        private S7.Net.Plc _S7Plc;
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
                        bool ConsoleWrite = ConsoleWriteWatch.ElapsedMilliseconds >= 5000;
                        if (ConsoleWrite) ConsoleWriteWatch.Restart();

                        Thread.Sleep(_AcquisitorThreadSleep);

                        Stopwatch Stopwatch = new Stopwatch();

                        // Read bytes from PLC
                        Stopwatch.Start();
                        foreach (S7db db in _S7dbs)
                        {
                            db.bytes = await _S7Plc.ReadBytesAsync(S7.Net.DataType.DataBlock, db.Num, 0, db.Len); // TODO Implement Async
                        }
                        Stopwatch.Stop();
                        if (ConsoleWrite) Console.WriteLine($"{_AcquisitorName} Reading bytes from S7PLC took: {Stopwatch.ElapsedMilliseconds} ms");
    
                        // Parse CVs
                        Stopwatch.Restart();
                        foreach (S7db db in _S7dbs)
                        {
                            db.ParseCVs(); // TODO Implement Async
                        }
                        Stopwatch.Stop();
                        if (ConsoleWrite) Console.WriteLine($"{_AcquisitorName} Parsing CVs from S7 PLC took: {Stopwatch.ElapsedMilliseconds} ms");

                        // Broadcast CVs to Clients
                        //MrgadaServerBroadcast(BroadcastBytes);

                    }
                    else
                    {
                        try 
                        { 
                            _S7Plc.Open();
                            IsConnected = true;
                        } catch 
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