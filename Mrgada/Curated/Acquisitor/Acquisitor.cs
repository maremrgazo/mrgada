#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using S7.Net;
using System.Net.Sockets;
using System.Net;

public static partial class Mrgada
{
    public enum AcquisitorType
    {
        S71500 = S7.Net.CpuType.S71500,
        S71200 = S7.Net.CpuType.S71200,
        OPCUA
    }

    private enum AcquisitorHighLevelType
    {
        S7,
        OPCUA
    }

    public partial class Acquisitor
    {
        public bool IsConnected = false;  

        // Constructor Variables
        public string _AcquisitorName;
        public AcquisitorType _AcquisitorType;
        public string _AcquisitorIp;
        public int _AcquisitorTcpPort;
        public int _AcquisitorThreadSleep;

        private TcpListener _AcquisitorTcpListener;
        private AcquisitorHighLevelType _AcquisitorHighLevelType;

        private bool AcquisitorStarted = false;
        public void StartAcquisition() 
        {
            if (AcquisitorStarted)
            {
                Console.WriteLine($"Acquisitor {_AcquisitorName} already started!");
                return;
            }
            AcquisitorStarted = true;
            // Switch based on MachineType (Server/Client)
            switch (Mrgada._MachineType)
            {

                case Mrgada.MachineType.Server:

                    InitializeTcpListener();
                    InitializeClientConnectHandlerThread();
                    InitializeAcquisitorHandlerThread();

                    break;

                case Mrgada.MachineType.Client:

                    InitializeClientConnectThread();

                    break;

            }
        }

        public Acquisitor(string AcquisitorName, AcquisitorType AcquisitorType, string AcquisitorIp, int AcquisitorTcpPort, int AcquisitorThreadSleep)
        {
            _AcquisitorName = AcquisitorName;
            _AcquisitorType = AcquisitorType;
            _AcquisitorIp = AcquisitorIp;
            _AcquisitorTcpPort = AcquisitorTcpPort;
            _AcquisitorThreadSleep = AcquisitorThreadSleep;

            // Set AcquisitorHighLevelType (S7/OPCUA/...)
            if
                (
                _AcquisitorType == AcquisitorType.S71200 ||
                _AcquisitorType == AcquisitorType.S71500
                )
                _AcquisitorHighLevelType = AcquisitorHighLevelType.S7;
            else if 
                (
                _AcquisitorType == AcquisitorType.OPCUA
                )
                _AcquisitorHighLevelType = AcquisitorHighLevelType.OPCUA;
        }
    }

}
