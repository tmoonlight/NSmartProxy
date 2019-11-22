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
        const int TargetPort = 30001;
        
        static async Task Main(string[] args)
        {
            await Task.Delay(2000);//慢点启动，以防服务端还没起来消息就发出去了
            Console.WriteLine("*** Udp Client ***");
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
                await udpClient.SendAsync(bytes, bytes.Length);
                Console.WriteLine($"发送{str}");
                await Task.Delay(1000);
                count--;
            }


        }

        private static async Task ReceiveUdpClientAsync(UdpClient udpClient)
        {
            while (true)
            {
                var udpReceiveResult = await udpClient.ReceiveAsync();//如果服务端没起来 这里将会永久阻塞，即使之后起来了也不行
                Console.WriteLine("接收到" + Encoding.ASCII.GetString(udpReceiveResult.Buffer));
            }

        }
    }
}
