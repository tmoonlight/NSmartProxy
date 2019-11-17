using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UdpServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("*** server ***");
            UdpClient udpClient = new UdpClient(6611);
            //UdpClient udpClient = new UdpClient(9999);
            while (true)
            {
                var udpReceiveResult = await udpClient.ReceiveAsync();
                var str = Encoding.ASCII.GetString(udpReceiveResult.Buffer);
                var receiveStr = Encoding.ASCII.GetBytes("hello" + str);
                udpClient.Send(receiveStr, receiveStr.Length, udpReceiveResult.RemoteEndPoint);
                Console.WriteLine($"[{udpReceiveResult.RemoteEndPoint.ToString()}]{str}");
            }

            //Console.WriteLine("Hello World!");
        }
    }
}
