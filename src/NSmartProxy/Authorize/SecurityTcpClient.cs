using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NSmartProxy.Infrastructure;

namespace NSmartProxy.Authorize
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// 标识位        token长度    token值
    /// 2位固定0xF9   2            n
    /// </summary>
    public class SecurityTcpClient : TcpClient
    {
        public string Token = "";
        public readonly byte F9 = 0xF9;//固定标识位
        public String ErrorMessage = "";

        public SecurityTcpClient(string secureToken) : base()
        {
            Token = secureToken;
        }

        /// <summary>
        /// 带加密串传输
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task ConnectWithAuthAsync(string host, int port)
        {
            if (String.IsNullOrEmpty(Token))
            {
                return;
            }

            //标识位 token长度 值
            int requestLength = 1 + 2 + Token.Length;

            var stream = this.GetStream();
            await base.ConnectAsync(host, port);
            await stream.WriteAsync(new byte[] { F9 });
            await stream.WriteAsync(StringUtil.IntTo2Bytes(Token.Length));
            await stream.WriteAndFlushAsync(ASCIIEncoding.ASCII.GetBytes(Token));
        }

        ////传输加密，待开发
        //public SslStream GetSslStream()
        //{
        //    return new SslStream(this.GetStream());
        //}

        /// <summary>
        /// 服务端校验
        /// </summary>
        /// <returns></returns>
        public async Task<AuthResult> AuthorizeAsync()
        {
            var stream = this.GetStream();
            //标识 1
            var protocolBytes = ArrayPool<byte>.Shared.Rent(1);
            if (await stream.ReadAsync(protocolBytes, 0, protocolBytes.Length) == 0)
            {
                ErrorMessage += "读取到0字节，客户端已关闭？";
                return null;
            }
            if (protocolBytes[0] != F9)
            {
                ErrorMessage += $"非法头部 {protocolBytes[0]}";
            }

            //2.token长度 2
            var lengthBytes = ArrayPool<byte>.Shared.Rent(2);
            if (await stream.ReadAsync(lengthBytes, 0, lengthBytes.Length) == 0)
            {
                ErrorMessage += "读取到0字节，客户端已关闭？";
                return null;
            }
            int tokenLength = StringUtil.DoubleBytesToInt(lengthBytes);

            //3.校验
            var tokenBytes = ArrayPool<byte>.Shared.Rent(tokenLength);
            if (await stream.ReadAsync(tokenBytes, 0, tokenBytes.Length) == 0)
            {
                ErrorMessage += "获取token失败？";
                return null;
            }

            var token = ASCIIEncoding.ASCII.GetString(tokenBytes);
            //TODO ***校验Token

            return new AuthResult()
            {
                Success = false
            };
        }
    }
}
