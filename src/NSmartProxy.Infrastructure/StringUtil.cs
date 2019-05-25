using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSmartProxy
{
    public static class StringUtil
    {
        /// <summary>
        /// 整型转双字节
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
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
            byte[] bytes = new byte[3];
            byte[] clientbytes = IntTo2Bytes(clientID);
            bytes[0] = clientbytes[0];
            bytes[1] = clientbytes[1];
            bytes[2] = (byte)appid;

            return bytes;
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

        public static int DoubleBytesToInt(byte[] bytes)
        {
            return (bytes[0] << 8) + bytes[1];
        }

        /// <summary>
        /// comma
        /// </summary>
        /// <param name="sb"></param>
        /// <returns></returns>
        public static StringBuilder C(this StringBuilder sb)
        {
            return sb.Append(",");
        }

        /// <summary>
        /// delcomma
        /// </summary>
        /// <param name="sb"></param>
        /// <returns></returns>
        public static StringBuilder D(this StringBuilder sb)
        {
            return sb.Remove(sb.Length - 1, 1);
        }

        public static dynamic ToDynamic(this string jsonString)
        {
            return JsonConvert.DeserializeObject(jsonString);
        }

        public static T ToObject<T>(this string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}
