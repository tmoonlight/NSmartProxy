using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            Router clientRouter = new Router();
            Task tsk = clientRouter.ConnectToProvider();
            Console.Read();
        }
    }
}
