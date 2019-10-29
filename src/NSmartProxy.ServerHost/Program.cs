using NSmartProxy.Shared;
using PeterKottas.DotNetCore.WindowsService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NSmartProxy.ServerHost
{
    public class Program
    {
        static void Main()
        {
            //wait
            ServiceRunner<ServerHost>.Run(config =>
            {
                var name = Global.NSPServerServiceName;
                config.SetDisplayName(Global.NSPServerServiceName);
                config.SetName(Global.NSPServerDisplayName);
                config.SetDescription(NSPVersion.NSmartProxyServerName);

                config.Service(serviceConfig =>
                {
                    serviceConfig.ServiceFactory((extraArguments, controller) =>
                    {
                        return new ServerHost();
                    });

                    serviceConfig.OnStart((service, extraParams) =>
                    {
                        Console.WriteLine("Service {0} started", name);
                        Task.Run(() => service.Start());
                    });

                    serviceConfig.OnStop(service =>
                    {
                        Console.WriteLine("Service {0} stopped", name);
                        Task.Run(() => service.Stop());
                    });

                    serviceConfig.OnError(e =>
                    {
                        Console.WriteLine("Service {0} errored with exception : {1}", name, e.Message);
                    });
                });


            });
        }
    }
}
