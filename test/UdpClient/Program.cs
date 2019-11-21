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
        const int TargetPort = 30000;
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("*** client ***");
            UdpClient udpClient = new UdpClient();
           
            udpClient.Connect("127.0.0.1", TargetPort);
            //udpClient.Connect("127.0.0.1", 9999 );
            _ = ReceiveUdpClientAsync(udpClient);
            int count = 5;
            while (count > 0)
            {
                string str = "test"+count;

                if (str == "0")
                {
                    udpClient.Close();
                    //udpClient.Connect("127.0.0.1", 9999);关了就没法再连了 必须new
                    continue;
                }

                var bytes = Encoding.ASCII.GetBytes(str);
                udpClient.Send(bytes, bytes.Length);
                System.Threading.Thread.Sleep(1000);
                count--;
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
