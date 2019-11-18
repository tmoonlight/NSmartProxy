using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using NSmartProxy.Data;
using NSmartProxy.Infrastructure;
using Snappy.Sharp;

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
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        public static byte[] ToASCIIBytes(this string str)
        {
            return System.Text.Encoding.ASCII.GetBytes(str);
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

        //From https://stackoverflow.com/questions/4619735/how-to-read-last-n-lines-of-log-file
        ///<summary>Returns the end of a text reader.</summary>
        ///<param name="reader">The reader to read from.</param>
        ///<param name="lineCount">The number of lines to return.</param>
        ///<returns>The last lneCount lines from the reader.</returns>
        public static string[] Tail(this TextReader reader, int lineCount)
        {
            var buffer = new List<string>(lineCount);
            string line;
            for (int i = 0; i < lineCount; i++)
            {
                line = reader.ReadLine();
                if (line == null) return buffer.ToArray();
                buffer.Add(line);
            }

            int lastLine = lineCount - 1;           //The index of the last line read from the buffer.  Everything > this index was read earlier than everything <= this indes

            while (null != (line = reader.ReadLine()))
            {
                lastLine++;
                if (lastLine == lineCount) lastLine = 0;
                buffer[lastLine] = line;
            }

            if (lastLine == lineCount - 1) return buffer.ToArray();
            var retVal = new string[lineCount];
            buffer.CopyTo(lastLine + 1, retVal, 0, lineCount - lastLine - 1);
            buffer.CopyTo(0, retVal, lineCount - lastLine - 1, lastLine + 1);
            return retVal;
        }


        /// <summary>
        /// 在一个byte数组中查找另外一个byte数组的位置
        /// </summary>
        /// <param name="haystack"></param>
        /// <param name="needle"></param>
        /// <returns></returns>
        public static int SearchBytesFromBytes(byte[] haystack, byte[] needle)
        {
            for (int i = 0; i <= haystack.Length - needle.Length; i++)
            {
                if (Match(haystack, needle, i))
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool Match(byte[] haystack, byte[] needle, int start)
        {
            if (needle.Length + start > haystack.Length)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < needle.Length; i++)
                {
                    if (needle[i] != haystack[i + start])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// 判断字符串是否是无小数点纯数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNum(this string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] < '0' || str[i] > '9')
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 使用snappy算法解压缩
        /// </summary>
        /// <param name="compressed"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] DecompressInSnappy(byte[] compressed, int offset, int length)
        {
            SnappyDecompressor sd = new SnappyDecompressor();
            try
            {
                return sd.Decompress(compressed, offset, length);
            }
            catch (Exception ex)
            {
                _ = ex;
                //啥情況？
                return null;
            }
        }

        /// <summary>
        /// 使用snappy算法压缩
        /// </summary>
        /// <param name="uncompressed"></param>
        /// <param name="offset"></param>
        /// <param name="uncompressedLength"></param>
        /// <returns></returns>
        public static CompressedBytes CompressInSnappy(byte[] uncompressed, int offset, int uncompressedLength)
        {
            SnappyCompressor sc = new SnappyCompressor();

            //var bytes = Encoding.ASCII.GetBytes("HelloWor134ertegsdfgsfdgsdfgsdfgsfdgsdfgsdfgsdfgsdfgdsfgsdfgdsfgdfgdsfgld");
            byte[] outBytes = new byte[sc.MaxCompressedLength(uncompressed.Length)];

            int actualLength = sc.Compress(uncompressed, 0, uncompressedLength, outBytes);
            return new CompressedBytes() { ContentBytes = outBytes, Length = actualLength };
        }

        /// <summary>
        /// 压缩专用对象
        /// </summary>
        public class CompressedBytes
        {
            public int Length;
            public byte[] ContentBytes;
        }
    }
}
