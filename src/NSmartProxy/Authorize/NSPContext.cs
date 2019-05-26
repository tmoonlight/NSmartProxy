using System;
using System.Collections.Generic;
using System.Text;
using NSmartProxy.Data;

namespace NSmartProxy.Authorize
{
    public class NSPContext
    {
        //服务端会话池，登陆后的会话都在这里，每天需要做定时清理

        public HashSet<string> TokenCaches;

        public NSPContext()
        {
            TokenCaches = new HashSet<string>();
        }

        //public 
        //public AuthState Authorize(string token)
        //{
        //   // StringUtil token
        //    //return true;
        //}

        //public AuthState Authorize(TokenClaims tkClaims)
        //{
            
        //    // StringUtil token
        //    //return true;
        //}

    }
}
