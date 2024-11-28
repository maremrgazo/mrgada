#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using S7.Net;
using System.Net.Sockets;
using System.Net;

using static System.Runtime.InteropServices.JavaScript.JSType;
using S7.Net.Types;
using System.Collections;

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
        public int _AcquisitorThreadInterval;

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

        public Acquisitor(string AcquisitorName, AcquisitorType AcquisitorType, string AcquisitorIp, int AcquisitorTcpPort, int AcquisitorThreadInterval)
        {
            _AcquisitorName = AcquisitorName;
            _AcquisitorType = AcquisitorType;
            _AcquisitorIp = AcquisitorIp;
            _AcquisitorTcpPort = AcquisitorTcpPort;
            _AcquisitorThreadInterval = AcquisitorThreadInterval;

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

        private void AcquisitorServerBroadcast(List<byte> BroadcastBytes)
        {
            if (BroadcastBytes.Count == 0) return;
            if (_Clients.Count == 0)
            {
                if (_ConsoleWrite) Console.WriteLine($"No Clients connected, Acquisitor {_AcquisitorName,-10}: didn't broadcast any bytes");
                return;
            }
            byte[] BroadcastBytesArray = BroadcastBytes.ToArray();
            foreach (TcpClient Client in _Clients)
            {
                try
                {
                    NetworkStream Stream = Client.GetStream();
                    Stream.Write(BroadcastBytesArray, 0, BroadcastBytesArray.Length);
                }
                catch (Exception)
                {
                    // Handle any exceptions (e.g., client disconnected during broadcast)
                    Console.WriteLine("Error while Acquisitor was broadcasting bytes!");
                    break;
                }
            }
            Console.WriteLine($"{_AcquisitorName, -8}: Acquisitor Broadcast bytes len ({BroadcastBytes.Count}) to {_Clients.Count} Clients!");
            //byte[] ByteLog = new byte[10];
            //Array.Copy(BroadcastBytesArray, ByteLog, Math.Min(BroadcastBytesArray.Length, ByteLog.Length));
            //string ByteLogString = BitConverter.ToString(ByteLog).Replace("-", "");

            //foreach (byte b in ByteLogString)
            //{
            //    if (_ConsoleWrite) Console.WriteLine(Convert.ToString(b, 2).PadLeft(8, '0')); // Convert to binary and pad to 8 bits
            //}

            //Console.WriteLine($"{_AcquisitorName, -10}: Acquisitor Broadcast following bytes: {ByteLogString}");
        }
    }

}
