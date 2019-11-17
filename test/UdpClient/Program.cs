using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp15
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("*** client ***");
            UdpClient udpClient = new UdpClient();
            //IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.1"), 17878);
            // IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127"), 9999);
            // udpClient.Client.Bind(ipEndPoint);
            //udpClient.Client.
            //_ =
            //udpClient.ReceiveAsync()
            //udpClient.Receive(ref new System.Net.IPEndPoint(IPAd));

            udpClient.Connect("127.0.0.1", 33333);
            //udpClient.Connect("127.0.0.1", 9999 );
            _ = ReceiveUdpClientAsync(udpClient);
            while (true)
            {
                string str = Console.ReadLine();

                if (str == "0")
                {
                    udpClient.Close();
                    //udpClient.Connect("127.0.0.1", 9999);关了就没法再连了 必须new
                    continue;
                }

                var bytes = Encoding.ASCII.GetBytes(str);
                udpClient.Send(bytes, bytes.Length);
            }


        }

        private static async Task ReceiveUdpClientAsync(UdpClient udpClient)
        {
            while (true)
            {
                var udpReceiveResult = await udpClient.ReceiveAsync();
                Console.WriteLine("接收到" + Encoding.ASCII.GetString(udpReceiveResult.Buffer));
            }

        }
    }
}
