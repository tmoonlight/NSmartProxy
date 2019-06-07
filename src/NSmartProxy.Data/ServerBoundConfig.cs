using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy.Data
{
    /// <summary>
    /// 服务的端口绑定配置以及用户的banlist
    /// </summary>
    [Serializable]
    public class ServerBoundConfig
    {
        public HashSet<string> UsersBanlist = new HashSet<string>();
        public Dictionary<string, UserPortBound> UserPortBounds = new Dictionary<string, UserPortBound>();

        
    }

    public class UserPortBound
    {
        public string UserId;
        public List<int> Bound = new List<int>();
    }
}
