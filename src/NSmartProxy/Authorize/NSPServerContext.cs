using System;
using System.Collections.Generic;
using System.Text;
using NSmartProxy.Data;

namespace NSmartProxy.Authorize
{
    public class NSPServerContext
    {
        //服务端会话池，登陆后的会话都在这里，每天需要做定时清理

        public HashSet<string> TokenCaches;
        private bool supportAnonymousLogin = true;

        public NSPServerContext()
        {
            TokenCaches = new HashSet<string>();
        }

        /// <summary>
        /// 支持客户端匿名登陆
        /// </summary>
        public bool SupportAnonymousLogin { get => supportAnonymousLogin; set => supportAnonymousLogin = value; }

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
