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
using log4net.Config;
using NSmartProxy.Data.Models;
using Exception = System.Exception;
using NSmartProxy.Shared;
using Protocol = NSmartProxy.Data.Protocol;
using NSmartProxy.Infrastructure;

namespace NSmartProxy
{
    class NSmartProxyClient
    {
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

        public static ILog Logger;
        public static IConfigurationRoot Configuration { get; set; }
        private static LoginInfo _currentLoginInfo;
        private static readonly string ConfigFilePath = ConfigHelper.AppSettingFullPath;
        public void Start(string[] args)
        {
            //appSettingFilePath = Directory.GetCurrentDirectory() + "/appsettings.json";
            //log
            var loggerRepository = LogManager.CreateRepository("NSmartClientRouterRepository");
            XmlConfigurator.Configure(loggerRepository, new FileInfo("log4net.config"));
            NSmartProxyClient.Logger = LogManager.GetLogger(loggerRepository.Name, "NSmartServerClient");
            if (!loggerRepository.Configured) throw new Exception("log config failed.");
            Console.ForegroundColor = ConsoleColor.Yellow;

            //用户登录 e.g.: ./NSmartProxyClient -u admin -p admin
            if (args.Length == 4)
            {
                _currentLoginInfo = new LoginInfo();
                _currentLoginInfo.UserName = args[1];
                _currentLoginInfo.UserPwd = args[3];
            }

            Logger.Info($"*** {NSPVersion.NSmartProxyClientName} ***");

            //start clientrouter.
            try
            {
                StartClient().Wait();
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
            Console.Read();
            Logger.Info("Client terminated,press any key to continue.");

        }

        private static async Task StartClient()
        {

            Router clientRouter = new Router(new Log4netLogger());
            //read config from config file.
            clientRouter.SetConfiguration(ConfigHelper.ReadAllConfig<NSPClientConfig>(ConfigFilePath));
            if (_currentLoginInfo != null)
            {
                clientRouter.SetLoginInfo(_currentLoginInfo);
            }

            Task tsk = clientRouter.Start(true);
            try
            {
                await tsk;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }

        }

        public void Stop()
        {
            //
            Console.WriteLine(NSPVersion.NSmartProxyServerName + " STOPPED.");
            Environment.Exit(0);
        }
    }
}
