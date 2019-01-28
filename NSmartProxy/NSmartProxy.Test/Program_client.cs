using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NSmartProxy.Test
{


    class Program
    {
        public static void Main(String[] args)
        {
            ////TcpListener listener = new TcpListener(IPAddress.Any, 6666);
            //TcpClient tcpClient = new TcpClient();
            //tcpClient.Connect(IPAddress.Parse("172.20.66.84"),80);
            TcpListener listener = new TcpListener(IPAddress.Any, 89);
            var client = listener.AcceptTcpClient();
            byte[] buf = new byte[1024];

            client.GetStream().Read(buf, 0, buf.Length);
        }
    }
}
