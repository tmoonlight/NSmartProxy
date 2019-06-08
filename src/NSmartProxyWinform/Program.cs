using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSmartProxyWinform
{

    static class Program
    {
        private static Mutex mutex = new Mutex(true, "{41ACBA9E-9699-4766-891B-57F325420A78}");

        public static IConfigurationRoot Configuration { get; set; }
        public static ILog Logger;

        public static string LoggerPath = "log4net.config";
        public static string LoggerConfigPath = "log4net.config";
        public const string LOGGER_REPO_NAME = "NSmartClientRouterRepository";
        public const string CONFIG_FILE_PATH = "./appsettings.json";

        //public static string[] Args;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (!mutex.WaitOne(3, false))
            {
                string msg = "Another instance of the program is running.";
                //Logger.Error(msg, new Exception(msg));
                MessageBox.Show(msg);
                return;
            }

            var loggerRepository = LogManager.CreateRepository(LOGGER_REPO_NAME);
            var config = XmlConfigurator.Configure(loggerRepository, new FileInfo(LoggerConfigPath));
            //BasicConfigurator.Configure(loggerRepository);
            Program.Logger = LogManager.GetLogger(loggerRepository.Name, "NSmartServerClient");
            if (!loggerRepository.Configured) throw new Exception("log config failed.");
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile(CONFIG_FILE_PATH);
            Configuration = builder.Build();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var clientForm = new ClientMngr();
            //Application.ApplicationExit += (sender, e) =>
            //{
            //    clientForm.Close();
            //};

            Application.Run(clientForm);
        }


    }
}
