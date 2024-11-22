public static partial class Mrgada 
{
    public class CODESYSPLC : Acquisitor
    {
        public CODESYSPLC(string AcquisitorName, AcquisitorType AcquisitorType, string AcquisitorIp, int AcquisitorTcpPort, int AcquisitorThreadSleep)
            : base(AcquisitorName, AcquisitorType, AcquisitorIp, AcquisitorTcpPort, AcquisitorThreadSleep)
        {
        }
        public OpcUaTag TEST;
        public override void InitializeOpcUaNodes()
        {
            string PLC_PRG = "ns=4;s=|var|CODESYS Control Win V3 x64.Application.PLC_PRG.";
            TEST = new($"{PLC_PRG}TEST", () => _OpcUaClient);
            _OpcUaTags.Add(TEST);
        }
    }
}