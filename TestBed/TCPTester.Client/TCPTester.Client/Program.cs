using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPTester.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect("192.168.1.2", 2344);
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

            while (true)
            {
                string str = Console.ReadLine();
                if (str == "c")
                {
                    tcpClient.Close();
                    break;
                }

                byte[] allbBytes = ASCIIEncoding.ASCII.GetBytes(str);
                stream.Write(allbBytes, 0, allbBytes.Length);
            }



        }
    }
}
