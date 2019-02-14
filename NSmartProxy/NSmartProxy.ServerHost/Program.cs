using System;
using System.Threading.Tasks;

namespace NSmartProxy.ServerHost
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("*** NSmart Server v0.1 ***");
            Server srv = new Server();
            Task.Run(async () =>
            {
                await srv.Start(); 
            }).GetAwaiter().GetResult();
            Console.WriteLine("NSmart server stopped. Press any key to continue.");
            Console.Read();
        }
    }
}
