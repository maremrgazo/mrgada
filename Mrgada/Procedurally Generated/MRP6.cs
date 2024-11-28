using Serilog;
using System.Net.Sockets;
using static Mrgada;

public static partial class Mrgada
{
    public class MRP6 : Acquisitor
    {
        public MRP6(string AcquisitorName, AcquisitorType AcquisitorType, string AcquisitorIp, int AcquisitorTcpPort, int AcquisitorThreadSleep)
            : base(AcquisitorName, AcquisitorType, AcquisitorIp, AcquisitorTcpPort, AcquisitorThreadSleep)
        {
            InitializeS7dbs();
        }

        public override void ParseAcquisitorBroadcast(byte[] Broadcast)
        {
            int i = 0;
            while (i < Broadcast.Length)
            {
                //byte[] dbNumberBytes = new byte[2];
                //Buffer.BlockCopy(BroadcastBuffer, i, dbNumberBytes, 0, 2);

                short SegmentLength = BitConverter.ToInt16(Broadcast, i);
                short dbNumber = BitConverter.ToInt16(Broadcast, i + 2);
                byte[] dbBytes = new byte[SegmentLength - 4];
                Buffer.BlockCopy(Broadcast, i + 4, dbBytes, 0, dbBytes.Length);

                switch (dbNumber)
                {
                    case 52:
                        dbDigitalValves.Bytes = dbBytes;
                        break;
                    case 51:
                        dbAnalogSensors.Bytes = dbBytes;
                        break;
                }
                Log.Information($"{_AcquisitorName,-10}: Recieved Bytes from S7 Acquisitor for db {dbNumber}, len {dbBytes.Length}");

                i += SegmentLength;

                if (Broadcast[i] == 0) break;
            }
        }

        public override void ReadS7dbs()
        {
            dbDigitalValves.Read();
            dbAnalogSensors.Read();
        }

        public override void OnClientConnect(TcpClient client)
        {
            lock (_S7ByteLock)
            {
                dbDigitalValves.OnClientConnect();
                dbAnalogSensors.OnClientConnect();
            }
        }

        public override void InitializeS7dbs()
        {
            dbDigitalValves = new c_dbDigitalValves(52, 791, _S7Plc, this);
            dbAnalogSensors = new(51, 2130, _S7Plc, this);
        }

        public c_dbDigitalValves dbDigitalValves;
        public S7db dbAnalogSensors;
        public class c_dbDigitalValves : S7db
        {
            public c_dbDigitalValves(int Num, int Len, S7.Net.Plc _S7Plc, Acquisitor _Acquisitor)
            : base(Num, Len, _S7Plc, _Acquisitor)
            {

            }

        }
    }
}
