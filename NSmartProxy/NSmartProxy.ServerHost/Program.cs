using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NSmartProxy.ServerHost
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        static void Main(string[] args)
        {
            Console.WriteLine("*** NSmart Server v0.1 ***");
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
            StartServer();
        }

        private static void StartServer()
        {
            try
            {
                Server.ClientServicePort = int.Parse(Configuration.GetSection("ClientServicePort").Value);
                Server.ConfigServicePort = int.Parse(Configuration.GetSection("ConfigServicePort").Value);
            }
            catch(Exception ex)
            {
                Console.WriteLine("配置文件读取失败：" + ex.ToString());
                return;
            }
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
