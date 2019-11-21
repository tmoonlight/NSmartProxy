using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UdpServer
{
    class Program
    {
        const int listenPort = 30000;

        static async Task Main(string[] args)
        {
            Console.WriteLine("*** server ***");
            UdpClient udpClient = new UdpClient(listenPort);
            //UdpClient udpClient = new UdpClient(9999);
            int count = 5;
            while (count > 0)
            {
                var udpReceiveResult = await udpClient.ReceiveAsync();
                var str = Encoding.ASCII.GetString(udpReceiveResult.Buffer);
                var receiveStr = Encoding.ASCII.GetBytes("hello" + str);
                udpClient.Send(receiveStr, receiveStr.Length, udpReceiveResult.RemoteEndPoint);
                Console.WriteLine($"[{udpReceiveResult.RemoteEndPoint.ToString()}]{str}");
                count--;
            }

            //Console.WriteLine("Hello World!");
        }
    }
}
