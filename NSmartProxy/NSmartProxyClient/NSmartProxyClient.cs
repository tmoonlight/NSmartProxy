using log4net;
using Microsoft.Extensions.Configuration;
using NSmartProxy.Client;
using NSmartProxy.Data;
using NSmartProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace NSmartProxy
{
    class NSmartProxyClient
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

        public static ILog Logger;
        public static IConfigurationRoot Configuration { get; set; }
        static void Main(string[] args)
        {
            //log
            NSmartProxyClient.Logger = LogManager.GetLogger(Assembly.GetEntryAssembly(), "NSmartServer");

            //Thread.Sleep(3000);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("*** NSmart ClientRouter v0.1 ***");

            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            //start clientrouter.
            try
            {
                StartClient().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }

            Console.WriteLine("client terminated,press any key to continure.");
            Console.Read();
        }

        private static async Task StartClient()
        {

            Router clientRouter = new Router(new Log4netLogger());
            //read config from config file.
            SetConfig(clientRouter);// clientRouter.SetConifiguration();
            Task tsk = clientRouter.ConnectToProvider();
            try
            {
                await tsk;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private static void SetConfig(Router clientRouter)
        {
            Config config = new Config();
            config.ProviderAddress = Configuration.GetSection("ProviderAddress").Value;
            config.ProviderPort = int.Parse(Configuration.GetSection("ProviderPort").Value);
            config.ProviderConfigPort = int.Parse(Configuration.GetSection("ProviderConfigPort").Value);
            var configClients = Configuration.GetSection("Clients").GetChildren();
            foreach (var cli in configClients)
            {
                config.Clients.Add(new ClientApp
                {
                    IP = cli["IP"],
                    TargetServicePort = int.Parse(cli["TargetServicePort"])
                });
            }
            // Configuration.GetSection("1").
            clientRouter.SetConifiguration(config);
        }
    }
}
