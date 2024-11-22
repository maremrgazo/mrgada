#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using S7.Net;
using System.Net.Sockets;
using System.Net;
using static Mrgada;

public static partial class Mrgada
{
    public partial class Acquisitor
    {
        private void InitializeAcquisitorHandlerThread()
        {
            switch (_AcquisitorHighLevelType)
            {
                case AcquisitorHighLevelType.S7:
                    InitializeS7AcquisitorHandlerThread();
                    break;
                case AcquisitorHighLevelType.OPCUA:
                    InitializeOPCUAAcquisitorHandlerThread();
                    break;
            }
        }
    }
}
