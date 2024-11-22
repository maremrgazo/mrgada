public static partial class Mrgada 
{
    public class CODESYSPLC : Acquisitor
    {
        public CODESYSPLC(string AcquisitorName, AcquisitorType AcquisitorType, string AcquisitorIp, int AcquisitorTcpPort, int AcquisitorThreadSleep)
            : base(AcquisitorName, AcquisitorType, AcquisitorIp, AcquisitorTcpPort, AcquisitorThreadSleep)
        {
        }
        public OpcUaTag<System.Int16> Timer;
        public OpcUaTag<float> AnalogSensor;
        public override void InitializeOpcUaNodes()
        {
            string PLC_PRG = "ns=4;s=|var|CODESYS Control Win V3 x64.Application.PLC_PRG.";
            AnalogSensor = new($"{PLC_PRG}AnalogSensor", () => _OpcUaClient);
            _OpcUaTags.Add(AnalogSensor);
            Timer = new($"{PLC_PRG}Timer", () => _OpcUaClient);
            _OpcUaTags.Add(Timer);
        }
    }
}