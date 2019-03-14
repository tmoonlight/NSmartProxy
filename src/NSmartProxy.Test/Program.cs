using System;
using System.Collections.Generic;
using System.Text;
using NSmartProxy.Data;

namespace NSmartProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            //ClientModel cm = new ClientModel();
            //cm.AppList = new List<App> { new App { AppId = 1, Port = 8091 }, new App { AppId = 2, Port = 8092 }, new App { AppId = 3, Port = 8093 } };
            //cm.ClientId = 1000;
            //byte[] d = cm.ToBytes();
            //ClientModel cm2 = ClientModel.GetFromBytes(d);


            ClientIdAppId cia = new ClientIdAppId();
            cia.AppId = 123;
            cia.ClientId = 2323;
            var bt = cia.ToBytes();
            var cia2 = ClientIdAppId.GetFromBytes(bt);
            Console.Read();
        }
    }
}
