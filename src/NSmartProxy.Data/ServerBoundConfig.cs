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
        public HashSet<string> UsersBanlist;
        public UserPortBound UserPortBoundList;

        public class UserPortBound
        {
            public string UserName;
            public List<int> Bound = new List<int>();
        }
    }
}
