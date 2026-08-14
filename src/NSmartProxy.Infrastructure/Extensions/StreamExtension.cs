using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
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
        public static async Task<int> ReadAsync(this Stream stream, byte[] buffer, int offset, int count, int timeOut)
        {
            //超时后必须让底层的读操作真正结束，否则这个读会一直挂在流上：
            //下一次读会和它并发（SslStream 会直接抛 NotSupportedException，
            //NetworkStream 则会造成数据交错），同时被丢弃的 Task 和定时器也会不断堆积。
            //这里通过关闭流来强制中止读操作——读超时本身就意味着这条连接已经不可用了。
            using (var timeoutCts = new CancellationTokenSource(timeOut))
            using (timeoutCts.Token.Register(s => SafeDispose((Stream)s), stream))
            {
                try
                {
                    return await stream.ReadAsync(buffer, offset, count, timeoutCts.Token).ConfigureAwait(false);
                }
                catch (Exception ex) when (timeoutCts.IsCancellationRequested &&
                                           (ex is OperationCanceledException || ex is IOException ||
                                            ex is ObjectDisposedException))
                {
                    return -1;//超时
                }
            }
        }

        private static void SafeDispose(Stream stream)
        {
            try { stream.Dispose(); }
            catch { /* 关闭失败无需处理 */ }
        }

        /// <summary>
        /// 读取接下来N字节的定长数据，如果服务端没有发那么多信息，
        /// 可能会出现读不全的情况，也有可能出现阻塞超时的情况
        /// </summary>
        public static async Task<int> ReadNextSTLengthBytes(this Stream stream, byte[] buffer)
        {
            int restBufferLength = buffer.Length;
            int totalReceivedBytes = 0;
            while (restBufferLength > 0)
            {
                int receivedBytes = await stream.ReadAsyncEx(buffer, totalReceivedBytes, restBufferLength);
                if (receivedBytes <= 0) return -1;//没有接收满则断开返回-1
                totalReceivedBytes += receivedBytes;
                restBufferLength -= receivedBytes;
            }
            return totalReceivedBytes;
        }

        public static async Task<int> ReadAsyncEx(this Stream stream, byte[] buffer, int offset, int count)
        {
            return await stream.ReadAsync(buffer, offset, count, Global.DefaultConnectTimeout);
        }

        public static async Task<int> ReadAsyncEx(this Stream stream, byte[] buffer)
        {
            return await stream.ReadAsyncEx(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 以服务端身份完成TLS握手。
        /// 握手必须是异步的：TLS握手需要多个网络往返，用同步的AuthenticateAsServer
        /// 会把线程池线程一直占住，并发上来以后线程池被耗尽，
        /// 表现就是整个服务端（包括侦听循环和其他隧道）一起卡死。
        /// 同时握手必须有超时：底层NetworkStream默认永不超时，
        /// 一个连上来却不发ClientHello的连接会永久占住这条连接的资源。
        /// </summary>
        public static async Task<Stream> ProcessSSLAsync(this Stream clientStream, X509Certificate2 cert,
            int handshakeTimeout = 0)
        {
            if (cert == null) throw new ArgumentNullException(nameof(cert));
            if (!cert.HasPrivateKey)
            {
                clientStream.Close();
                throw new InvalidOperationException(
                    $"证书“{cert.Subject}”不包含私钥，无法用于服务端TLS。请上传带私钥的pfx/p12证书。");
            }

            if (handshakeTimeout <= 0) handshakeTimeout = Global.SslHandshakeTimeout;

            SslStream sslStream = new SslStream(clientStream, false);
            try
            {
                //握手超时后关闭底层流，强制中止握手
                using (var timeoutCts = new CancellationTokenSource(handshakeTimeout))
                using (timeoutCts.Token.Register(s => SafeDispose((Stream)s), clientStream))
                {
                    try
                    {
#if NET6_0_OR_GREATER
                        var sslOptions = new SslServerAuthenticationOptions
                        {
                            ServerCertificate = cert,
                            ClientCertificateRequired = false,
                            //SslProtocols.None表示交给操作系统协商（TLS 1.2/1.3），
                            //TLS 1.3比写死的1.2少一个握手往返。
                            EnabledSslProtocols = SslProtocols.None,
                            //服务端不做客户端证书校验，开启吊销检查只会引入一次
                            //可能长时间阻塞的CRL/OCSP网络请求。
                            CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                        };
                        await sslStream.AuthenticateAsServerAsync(sslOptions, timeoutCts.Token).ConfigureAwait(false);
#else
                        await sslStream.AuthenticateAsServerAsync(cert, false, SslProtocols.Tls12, false)
                            .ConfigureAwait(false);
#endif
                    }
                    catch (Exception ex) when (timeoutCts.IsCancellationRequested)
                    {
                        throw new TimeoutException($"TLS握手超时（{handshakeTimeout}ms）。", ex);
                    }
                }

                //注意：这里刻意不设置Read/WriteTimeout。
                //这是一条长连接隧道，浏览器的keep-alive连接可以长时间空闲，
                //给它加一个固定的读超时会把正常的空闲连接打断。
                return sslStream;
            }
            catch
            {
                SafeDispose(sslStream);
                SafeDispose(clientStream);
                throw;
            }
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
        /// 写入动态长度的字节，头四字节存放长度
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static async Task WriteQLengthBytes(this Stream stream, byte[] bytes,int forceLength = -1)
        {
            if (forceLength > 0)
            {
                stream.Write(StringUtil.IntTo4Bytes(forceLength), 0, 4);
                await stream.WriteAsync(bytes,0,forceLength);
            }
            else
            {
                stream.Write(StringUtil.IntTo4Bytes(bytes.Length), 0, 4);
                await stream.WriteAsync(bytes);
            }
            
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
            var readByte = await stream.ReadAsync(bt2, 0, 2);
            byte[] bytes = null;
            if (readByte > 0)
            {
                int length = BitConverter.ToInt16(bt2, 0);
                bytes = new byte[length];
                await stream.ReadAsync(bytes, 0, length);
            }
            return bytes;
        }

        /// <summary>
        /// 读取动态长度的字节,头四字节存放长度,这种方式最大支持2G的数据
        /// 这种比ReadNextDLengthBytes支持更大数据，但是读取时候会造成中断，需要多次读取
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static async Task<byte[]> ReadNextQLengthBytes(this Stream stream)
        {
            byte[] bt2 = new byte[4];
            var readByte = await stream.ReadAsync(bt2, 0, 4);
            byte[] bytes = null;
            if (readByte > 0)
            {
                int length = BitConverter.ToInt32(bt2, 0);
                //长度头来自对端，必须校验，否则一个畸形的长度就会造成一次超大内存分配
                if (length < 0 || length > Global.MaxTransferFrameSize)
                {
                    throw new IOException($"非法的数据帧长度：{length}");
                }
                if (length == 0) return new byte[0];
                bytes = new byte[length];
                int readedByteCount = await stream.ReadAsync(bytes, 0, length);
                if (readedByteCount == 0)
                {
                    return new byte[0];
                }
                int restLength = length - readedByteCount;
                while (restLength > 0)
                {
                    readedByteCount += await stream.ReadAsync(bytes, readedByteCount, restLength);
                    if (readedByteCount == 0)
                    {
                        return new byte[0];
                    }
                    restLength = length - readedByteCount;
                }
            }
            else
            {
                return new byte[0];
            }
            return bytes;
        }

    }
}