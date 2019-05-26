using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSmartProxy.Data;
using NSmartProxy.Infrastructure;

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

        public static string ToASCIIString(this byte[] bytes)
        {
            return System.Text.ASCIIEncoding.ASCII.GetString(bytes);
        }

        public static byte[] ToASCIIBytes(this string str)
        {
            return System.Text.ASCIIEncoding.ASCII.GetBytes(str);
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

        public static TokenClaims ConvertStringToTokenClaims(string token)
        {
            try
            {
                token = EncryptHelper.AES_Decrypt(token);
                string[] tokenarr = token.Split('|');
                TokenClaims tkClaims = new TokenClaims();
                tkClaims.UserKey = tokenarr[0];
                try
                {
                    tkClaims.LastTime = DateTime.Parse(tokenarr[1]);
                }
                catch
                {
                    tkClaims.LastTime = new DateTime(2500, 1, 1);
                }

                return tkClaims;
            }
            catch (Exception ex)
            {
                throw new Exception("token格式不正确" + ex.ToString());
            }

        }
    }
}
