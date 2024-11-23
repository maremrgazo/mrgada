public static partial class Mrgada
{
    public class MRP6 : Acquisitor
    {
        public MRP6(string AcquisitorName, AcquisitorType AcquisitorType, string AcquisitorIp, int AcquisitorTcpPort, int AcquisitorThreadSleep)
            : base(AcquisitorName, AcquisitorType, AcquisitorIp, AcquisitorTcpPort, AcquisitorThreadSleep)
        {
        }

        public override void ReadS7dbs()
        {
            dbDigitalValves.Read();
        }

        public override void InitializeS7dbs()
        {
            dbDigitalValves = new c_dbDigitalValves(52, 200, _S7Plc);
        }

        c_dbDigitalValves dbDigitalValves;
        public class c_dbDigitalValves : S7db
        {
            public c_dbDigitalValves(int Num, int Len, S7.Net.Plc _S7Plc)
            : base(Num, Len, _S7Plc)
            {

            }

        }
    }
}
