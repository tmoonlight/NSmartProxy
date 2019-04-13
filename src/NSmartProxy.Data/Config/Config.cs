using NSmartProxy.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy.Data
{ 
    public class Config
    {
        public int ProviderPort;                    //代理转发服务端口
        public int ProviderConfigPort;              //配置服务端口
        public string ProviderAddress;              //代理服务器地址
        public List<ClientApp> Clients = new List<ClientApp>();//客户端app
    }
}
