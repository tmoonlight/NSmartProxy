using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UdpServer
{
    class Program
    {
        const int listenPort = 30001;

        static async Task Main(string[] args)
        {
            Console.WriteLine("*** Udp Sever ***");
            UdpClient udpClient = new UdpClient(listenPort);
            //UdpClient udpClient = new UdpClient(9999);
            int count = 5;
            while (count > 0)
            {
                var udpReceiveResult = await udpClient.ReceiveAsync();
                var str = Encoding.ASCII.GetString(udpReceiveResult.Buffer);
                var receiveStr = Encoding.ASCII.GetBytes("hello" + str);
                await udpClient.SendAsync(receiveStr, receiveStr.Length, udpReceiveResult.RemoteEndPoint);
                Console.WriteLine($"[{udpReceiveResult.RemoteEndPoint.ToString()}]{str}");
                count--;
            }

            //Console.WriteLine("Hello World!");
        }
    }
}
