using Microsoft.Extensions.Configuration;
using NSmartProxy.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using System.Threading;
using NSmartProxy.Shared;

namespace NSmartProxy.ServerHost
{
    class ServerHost
    {

        private static Mutex mutex = new Mutex(true, "{8639B0AD-A27C-4F15-B3D9-08035D0FC6D6}");
        #region logger
        public class Log4netLogger : INSmartLogger
        {
            public void Debug(object message)
            {
                //Logger.Debug(message);
                Logger.Debug(message);
            }

            public void Error(object message, Exception ex)
            {
                //Logger.Debug(message);
                Logger.Error(message, ex);
            }

            public void Info(object message)
            {
                Logger.Info(message);
            }
        }
        #endregion

        public static IConfigurationRoot Configuration { get; set; }
        public static ILog Logger;

        static void Main(string[] args)
        {
            if (!mutex.WaitOne(3, false))
            {
                string msg = "Another instance of the program is running.";
                //Logger.Error(msg, new Exception(msg));
                Console.Write(msg);
                return;
            }

            //log
            var loggerRepository = LogManager.CreateRepository("NSmartServerRepository");
            XmlConfigurator.ConfigureAndWatch(loggerRepository, new FileInfo("log4net.config"));
            Logger = LogManager.GetLogger(loggerRepository.Name, "NSmartServer");
            if (!loggerRepository.Configured) throw new Exception("log config failed.");

            Logger.Debug($"*** {Global.NSmartProxyServerName} ***");
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
                Logger.Debug("配置文件读取失败：" + ex.ToString());
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
                       .SetAnonymousLogin(true)
                       .Start()
                       .Wait();
                }
                catch (Exception ex)
                {
                    Logger.Debug(ex.ToString());
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
                if (retryCount > 10) break;

            }


            Logger.Debug("NSmart server terminated. Press any key to continue.");
            try
            {
                //只是为了服务器挂了不那么快退出进程而已
                Console.Read();
            }
            catch
            {
                // ignored
            }
        }
    }

    internal class NSmartProxyClient
    {
    }
}
