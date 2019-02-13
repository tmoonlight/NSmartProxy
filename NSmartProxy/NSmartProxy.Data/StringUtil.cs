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
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(number / 256);
            bytes[1] = (byte)(number % 256);
            return bytes;
        }

        /// <summary>
        /// 客户端首次连接服务端时，需要发送标记以便服务端归类
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="appid"></param>
        /// <returns></returns>
        public static byte[] ClientIDAppIdToBytes(int clientID, int appid)
        {
            byte[] bytes = IntTo2Bytes(clientID);
            bytes[2] = (byte)appid;

            return bytes.Take(3).ToArray();
        }

        /// <summary>
        /// 双字节转整型
        /// </summary>
        /// <param name="hByte"></param>
        /// <param name="lByte"></param>
        /// <returns></returns>
        public static int DoubleBytesToInt(byte hByte, byte lByte)
        {
            return (hByte << 8) + lByte;
        }
    }
}
