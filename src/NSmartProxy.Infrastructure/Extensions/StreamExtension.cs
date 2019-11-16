using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NSmartProxy.Shared;

namespace NSmartProxy.Infrastructure
{
    public static class StreamExtension
    {
        public static async Task WriteAndFlushAsync(this Stream stream, byte[] buffer, int offset = 0, int count = 0)
        {
            //不让赋值默认值，只能暂时给个0
            if (count == 0) count = buffer.Length;
            await stream.WriteAsync(buffer, offset, count);
            await stream.FlushAsync();
        }


        /// <summary>
        /// 带超时的readasync,timeout 毫秒
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="TimeOut"></param>
        /// <returns></returns>
        public static async Task<UdpReceiveResult?> ReceiveAsync(this UdpClient client, int timeOut)
        {
            UdpReceiveResult? udpReceiveResult = null;
            var receiveTask = Task.Run(async () => { udpReceiveResult = await client.ReceiveAsync(); });
            var isReceived = await Task.WhenAny(receiveTask, Task.Delay(timeOut)) == receiveTask;
            if (!isReceived) return null;
            return udpReceiveResult;
        }

        /// <summary>
        /// 带超时的readasync,timeout 毫秒
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="TimeOut"></param>
        /// <returns></returns>
        public static async Task<int> ReadAsync(this NetworkStream stream, byte[] buffer, int offset, int count, int timeOut)
        {
            var receiveCount = 0;
            var receiveTask = Task.Run(async () => { receiveCount = await stream.ReadAsync(buffer, offset, count); });
            var isReceived = await Task.WhenAny(receiveTask, Task.Delay(timeOut)) == receiveTask;
            if (!isReceived) return -1;
            return receiveCount;
        }


        public static async Task<int> ReadAsyncEx(this NetworkStream stream, byte[] buffer)
        {
            return await stream.ReadAsync(buffer, 0, buffer.Length, Global.DefaultConnectTimeout);
        }

        public static Stream ProcessSSL(this Stream clientStream, X509Certificate cert)
        {
            try
            {
                SslStream sslStream = new SslStream(clientStream);
                sslStream.AuthenticateAsServer(cert, false, SslProtocols.Tls, true);
                sslStream.ReadTimeout = 10000;
                sslStream.WriteTimeout = 10000;
                return sslStream;
            }
            catch (Exception ex)
            {
                clientStream.Close();
                throw ex;
            }

            //return null;
        }

        public static async Task WriteAsync(this Stream stream, byte[] bytes)
        {
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 写入字符串（ASCII）
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static async Task WriteDLengthBytes(this Stream stream, string asciiStr)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(asciiStr);
            stream.Write(StringUtil.IntTo2Bytes(bytes.Length), 0, 2);
            await stream.WriteAsync(bytes);
        }
        /// <summary>
        /// 写入动态长度的字节，头两字节存放长度
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static async Task WriteDLengthBytes(this Stream stream, byte[] bytes)
        {
            stream.Write(StringUtil.IntTo2Bytes(bytes.Length), 0, 2);
            await stream.WriteAsync(bytes);
        }

        /// <summary>
        /// 读取动态长度的字节，头两字节存放长度
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static async Task<byte[]> ReadNextDLengthBytes(this Stream stream)
        {
            // int readInt = 0; 
            byte[] bt2 = new byte[2];
            //readInt += bt2.Length;
            var readByte = stream.Read(bt2, 0, 2);
            byte[] bytes = null;
            if (readByte > 0)
            {//TODO 7这种写法会不会有问题
                bytes = new byte[readByte];
                await stream.ReadAsync(bytes, 0, StringUtil.DoubleBytesToInt(bt2));
            }
            return bytes;
        }

    }
}