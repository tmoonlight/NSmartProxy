using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPTester.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect("127.0.0.1", 5944);
            //tcpClient.Connect("192.168.1.2", 12306);
            var stream = tcpClient.GetStream();

            Task.Run(() =>
            {

                byte[] buffer = new byte[4096];
                while (true)
                {
                    int readResultLength = stream.Read(buffer, 0, buffer.Length);

                    //if(readResultLength)
                    Console.WriteLine(ASCIIEncoding.ASCII.GetString(buffer, 0, readResultLength).Trim());
                }
            });
            int x = 0;
            int count = 0;
            //数据传输测试
            while (count < 6)
            {
                string str = "test" + count;//Console.ReadLine();
                if (str == "test5")
                {
                    tcpClient.Close();
                    break;
                }

                byte[] allbBytes = ASCIIEncoding.ASCII.GetBytes(str + x);
                stream.WriteTimeout = 3000;
                stream.Write(allbBytes, 0, allbBytes.Length);
                //x++;
                //Console.ReadLine();
                Thread.Sleep(1000);
                count++;
            }
            //异常数据测试

        }
    }
}
