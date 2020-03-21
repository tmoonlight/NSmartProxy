using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy.Data.Config
{
    public class NSPServerConfig
    {
        public int ReversePort = 19974;   //服务端代理转发端口
        public int ConfigPort = 12308;    //服务端配置通讯端口
        public int WebAPIPort = 12309;    //远端管理端口
        public int ReversePort_Out = 0;
        public int ConfigPort_Out = 0;
        public bool supportAnonymousLogin = true;

        //[]
        public ServerBoundConfig BoundConfig = new ServerBoundConfig();//用户端口绑定列表
        //public List<CABoundConfig> CABoundConfigList = new List<CABoundConfig>();//host->证书路径
        public Dictionary<string,string> CABoundConfig = new Dictionary<string, string>();//证书绑定列表 端口->证书路径，为了方便扩展这里使用字符串类型
    }
}
