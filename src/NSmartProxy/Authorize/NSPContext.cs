using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy.Authorize
{
    public class NSPContext
    {
        //服务端会话池，登陆后的会话都在这里

        public HashSet<string> TokenCaches;

        public NSPContext()
        {
            TokenCaches = new HashSet<string>();
        }

        //public 
        public bool Authorize(string token)
        {
            return true;
        }
    }
}
