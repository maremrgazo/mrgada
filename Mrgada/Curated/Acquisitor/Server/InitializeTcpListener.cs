#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using S7.Net;
using System.Net.Sockets;
using System.Net;


public static partial class Mrgada
{
    public partial class Acquisitor
    {
        
        public void InitializeTcpListener() 
        {
            IPAddress MrgadaServerIp = IPAddress.Parse(Mrgada._ServerIp);
            _AcquisitorTcpListener = new TcpListener(MrgadaServerIp, _AcquisitorTcpPort);
            _AcquisitorTcpListener.Start();
            Console.WriteLine($"{_AcquisitorName,-10}: Acquisitor TCP Server Started!");
            //Console.WriteLine($"{_AcquisitorName} Acquisitor TCP Server Started!");
        }
          
    }

}
