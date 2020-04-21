using NSmartProxy.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NSmartProxy.Data
{
    public class NSPClientConfig
    {
        [JsonIgnore]
        public int ReversePort;                    //代理转发服务端口
        [JsonIgnore]
        public int ConfigPort;              //配置服务端口

        public bool UseServerControl = true; //启用服务端配置
        public string ProviderAddress;              //代理服务器地址
        public int ProviderWebPort;                    //web管理端的端口，默认12309 //TODO 暂时写死，以后再改
        public List<ClientApp> Clients = new List<ClientApp>();//客户端app
    }
}
