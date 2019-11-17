using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace NSmartProxyWinService
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                NSPClientService testService = new NSPClientService();
                testService.TestStartupAndStop(args);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new NSPClientService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
