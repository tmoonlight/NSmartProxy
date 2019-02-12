using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSmartProxy
{
    public class StringUtil
    {
        public static byte[] IntTo2Bytes(int number)
        {
            return System.BitConverter.GetBytes(number).Take(2).ToArray();
        }

        /// <summary>
        /// 客户端首次连接服务端时，需要发送标记以便服务端归类
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="appid"></param>
        /// <returns></returns>
        public static byte[] Generate1stRequestBytes(int clientID, int appid)
        {
            byte[] bytes = IntTo2Bytes(clientID);
            bytes[2] = (byte)appid;

            return bytes.Take(3).ToArray();
        }
    }
}
