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
            Console.WriteLine("*** Tcp Client ***");
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect("127.0.0.1", 64321);
            //tcpClient.Connect("192.168.1.2", 12306);
            var stream = tcpClient.GetStream();

            Task.Run(() =>
            {

                byte[] buffer = new byte[4096];
                while (true)
                {
                    try
                    {
                        int readResultLength = stream.Read(buffer, 0, buffer.Length);

                        if (readResultLength == 0) break;
                        Console.WriteLine(ASCIIEncoding.ASCII.GetString(buffer, 0, readResultLength).Trim());
                    }
                    catch
                    {
                        Console.WriteLine("stream closed ungracefully");
                        break;
                    }
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

            Console.WriteLine("*** Test null port ***");
            //异常数据测试

        }
    }
}
