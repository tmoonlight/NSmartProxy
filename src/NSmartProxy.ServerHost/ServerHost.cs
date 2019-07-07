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
using NSmartProxy.Data.Config;
using NSmartProxy.Infrastructure;
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

        public const string CONFIG_FILE_PATH = "./appsettings.json";
        static void Main(string[] args)
        {
            if (!mutex.WaitOne(3, false))
            {
                string msg = "Another instance of the program is running.It may cause fatal error.";
                //Logger.Error(msg, new Exception(msg));
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(msg);
                //return;
                //Console.ForegroundColor = default(ConsoleColor);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Initializing..");
            //log
            var loggerRepository = LogManager.CreateRepository("NSmartServerRepository");
            XmlConfigurator.ConfigureAndWatch(loggerRepository, new FileInfo("log4net.config"));
            Logger = LogManager.GetLogger(loggerRepository.Name, "NSmartServer");
            if (!loggerRepository.Configured) throw new Exception("log config failed.");

            Logger.Debug($"*** {Global.NSmartProxyServerName} ***");
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile(CONFIG_FILE_PATH);

            Configuration = builder.Build();
            StartServer();
        }

        private static void StartServer()
        {
            //try
            //{
            //    Server.ReversePort = int.Parse(Configuration.GetSection("ReversePort").Value);
            //    Server.ConfigPort = int.Parse(Configuration.GetSection("ConfigPort").Value);
            //}
            //catch (Exception ex)
            //{
            //    Logger.Debug("配置文件读取失败：" + ex.ToString());
            //    return;
            //}

            NSPServerConfig serverConfig = null;
            //初始化配置
            if (!File.Exists(CONFIG_FILE_PATH))
            {
                serverConfig = new NSPServerConfig();
                serverConfig.SaveChanges(CONFIG_FILE_PATH);
            }
            else
            {
                serverConfig = ConfigHelper.ReadAllConfig<NSPServerConfig>(CONFIG_FILE_PATH);
            }

            

            Server srv = new Server(new Log4netLogger());

            int retryCount = 0;
            while (true)
            {
                var watch = new Stopwatch();

                try
                {
                    watch.Start();
                    srv//.SetWebPort(int.Parse(Configuration.GetSection("WebAPIPort").Value))
                       .SetConfiguration(serverConfig)
                       .SetAnonymousLogin(true)
                       .SetServerConfigPath(CONFIG_FILE_PATH)
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
}
