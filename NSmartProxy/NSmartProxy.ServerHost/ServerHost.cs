using Microsoft.Extensions.Configuration;
using NSmartProxy.Interfaces;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;

namespace NSmartProxy.ServerHost
{
    class ServerHost
    {
        public class Log4netLogger : INSmartLogger
        {
            public void Debug(string message)
            {
                Logger.Debug(message);
            }

            public void Error(string message, Exception ex)
            {
                Logger.Error(message);
            }
        }

        public static IConfigurationRoot Configuration { get; set; }
        public static ILog Logger;
        static void Main(string[] args)
        {
            //log
            ServerHost.Logger = LogManager.GetLogger(Assembly.GetEntryAssembly(), "NSmartServer");

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
            catch (Exception ex)
            {
                Console.WriteLine("配置文件读取失败：" + ex.ToString());
                return;
            }
            Server srv = new Server(new Log4netLogger());

            Task.Run(async () =>
            {
                await srv.Start();
            }).GetAwaiter().GetResult();
            Console.WriteLine("NSmart server stopped. Press any key to continue.");
            Console.Read();
        }
    }
}
