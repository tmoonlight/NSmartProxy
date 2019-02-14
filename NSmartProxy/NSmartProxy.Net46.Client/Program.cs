using Microsoft.Extensions.Configuration;
using NSmartProxy.Client;
using NSmartProxy.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSmartProxy
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        static void Main(string[] args)
        {
            Thread.Sleep(3000);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("*** NSmart ClientRouter v0.1 ***");

            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            //start clientrouter.
            StartServer();
        }

        private static void StartServer()
        {
            Router clientRouter = new Router();
            //read config from config file.
            SetConfig(clientRouter);// clientRouter.SetConifiguration();
            Task tsk = clientRouter.ConnectToProvider();
            Console.Read();
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
