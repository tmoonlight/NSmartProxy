using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy
{
    public class StringUtil
    {
        public static byte[] IntToBytes(int number)
        {
            return System.BitConverter.GetBytes(number);
        }

        /// <summary>
        /// 客户端首次连接服务端时，需要发送标记以便服务端归类
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="appid"></param>
        /// <returns></returns>
        public static byte[] Generate1stRequestBytes(int clientID, int appid)
        {
            byte[] bytes = IntToBytes(clientID);
            bytes[2] = (byte)appid;
            bytes[3] = 0;
            return bytes;
        }
    }
}
