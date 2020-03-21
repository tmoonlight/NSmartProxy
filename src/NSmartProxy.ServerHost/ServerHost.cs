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
using PeterKottas.DotNetCore.WindowsService.Interfaces;

namespace NSmartProxy.ServerHost
{
    public class ServerHost : IMicroService
    {

        private static Mutex mutex = new Mutex(true, "{8639B0AD-A27C-4F15-B3D9-08035D0FC6D6}");
        private static ILog Logger;

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

        public IConfigurationRoot Configuration { get; set; }

        private static readonly string ConfigFilePath = ConfigHelper.AppSettingFullPath;

        public void Start()
        {
            if (!mutex.WaitOne(3, false))
            {
                //如果启动多个实例，则警告
                string msg = "Another instance of the program is running.It may cause fatal error.";
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(msg);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Initializing..");
            //log
            InitLogConfig();
            StartNSPServer();
        }

        private void InitLogConfig()
        {
            var loggerRepository = LogManager.CreateRepository("NSmartServerRepository");
            XmlConfigurator.ConfigureAndWatch(loggerRepository, new FileInfo("log4net.config"));
            Logger = LogManager.GetLogger(loggerRepository.Name, "NSmartServer");
            if (!loggerRepository.Configured) throw new Exception("log config failed.");

            Logger.Debug($"*** {NSPVersion.NSmartProxyServerName} ***");
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile(ConfigFilePath);

            Configuration = builder.Build();
        }

        private static void StartNSPServer()
        {
            NSPServerConfig serverConfig = null;
            //初始化配置
            if (!File.Exists(ConfigFilePath))
            {
                serverConfig = new NSPServerConfig();
                serverConfig.SaveChanges(ConfigFilePath);
            }
            else
            {
                serverConfig = ConfigHelper.ReadAllConfig<NSPServerConfig>(ConfigFilePath);
            }



            Server srv = new Server(new Log4netLogger());

            int retryCount = 0;
            while (true)
            {
                var watch = new Stopwatch();

                try
                {
                    watch.Start();
                    srv
                       .SetConfiguration(serverConfig)
                       //.SetAnonymousLogin(false) 从配置文件获取
                       .SetServerConfigPath(ConfigFilePath)
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

        public void Stop()
        {
            //
            Console.WriteLine(NSPVersion.NSmartProxyClientName + " STOPPED.");
            Environment.Exit(0);
        }
    }
}
