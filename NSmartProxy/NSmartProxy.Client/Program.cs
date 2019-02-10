using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NSmartProxy.Client
{
    class Program
    {
        /// <summary>
        /// Server端，用来和Proxy建立连接
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("***ClientServer***");
            Thread.Sleep(3000);
            Console.ForegroundColor = ConsoleColor.Yellow;
            ClientRouter clientRouter = new ClientRouter();
            clientRouter.ConnectToProvider();
            Console.Read();
        }
    }
}
