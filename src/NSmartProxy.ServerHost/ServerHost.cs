using Microsoft.Extensions.Configuration;
using NSmartProxy.Interfaces;
using System;
using System.Diagnostics;
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
                //Console.WriteLine(message);
                Logger.Debug(message);
            }

            public void Error(string message, Exception ex)
            {
                //Console.WriteLine(message);
                Logger.Error(message,ex);
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

            int retryCount = 0;
            while (true)
            {
                var watch = new Stopwatch();

                try
                {
                    watch.Start();
                    srv.SetWebPort(int.Parse(Configuration.GetSection("WebAPIPort").Value))
                       .Start()
                       .Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    watch.Stop();
                }

                //短时间多次出错则终止服务器
                if (watch.Elapsed > TimeSpan.FromSeconds(10))
                {
                    retryCount = 0;
                }
                else
                {
                    retryCount++;
                }
                if (retryCount > 100) break;

            }


            Console.WriteLine("NSmart server terminated. Press any key to continue.");
            try
            {
                Console.Read();
            }
            catch
            {
                // ignored
            }
        }
    }
}
