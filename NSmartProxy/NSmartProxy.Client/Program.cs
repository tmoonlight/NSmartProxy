using System;
using System.Net.Sockets;
using System.Text;

namespace NSmartProxy.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Try Connect..");
            TcpClient tc = new TcpClient(AddressFamily.InterNetwork);
            tc.Connect("127.0.0.1",8077);
            var netStream = tc.GetStream();//
            netStream.Write(Encoding.ASCII.GetBytes("Hello SHao1<EOF>"));
            netStream.Write(Encoding.ASCII.GetBytes("Hello SHao2<EOF>"));
           // netStream.Write(Encoding.ASCII.GetBytes("Hello SHao3<EOF>"));
            netStream.Flush();
            tc.Close();
        }
    }
}
