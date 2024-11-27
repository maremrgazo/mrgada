#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using S7.Net;
using System.Net.Sockets;
using System.Net;
using Serilog;

public static partial class Mrgada
{
    public partial class Acquisitor
    {
        
        public void InitializeTcpListener() 
        {
            IPAddress MrgadaServerIp = IPAddress.Parse(Mrgada._ServerIp);
            _AcquisitorTcpListener = new TcpListener(MrgadaServerIp, _AcquisitorTcpPort);
            _AcquisitorTcpListener.Start();
            Log.Information($"{_AcquisitorName,-10}: Acquisitor TCP Server Started!");
            //Log.Information($"{_AcquisitorName} Acquisitor TCP Server Started!");
        }
          
    }

}
