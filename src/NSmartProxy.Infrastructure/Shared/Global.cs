using NSmartProxy.Interfaces;

namespace NSmartProxy.Shared
{
    /// <summary>
    /// 放一些全局公用的静态变量
    /// </summary>
    public sealed class Global
    {
        public const string NO_TOKEN_STRING = "notoken";

        public const string TOKEN_COOKIE_NAME = "NSPTK";
        //public const string NSmartProxyClientName = "NSmartProxy Client v1.1.1028";
        //public const string NSmartProxyServerName = "NSmartProxy Server v1.1.1028";

        public const int ClientReconnectInterval = 3000;//客户端断线重连时间间隔（毫秒）

        public const int HeartbeatInterval = 30000;  //心跳间隔（毫秒）
        public const int HeartbeatCheckInterval = 90000;  //心跳检测间隔（毫秒）
        public const int DefaultConnectTimeout = 30000; //默认连接超时时间（毫秒）
        public const int DefaultWriteAckTimeout = 10000;//调用具备ack确认协议的等待时间（毫秒）
        public const int DefaultPopClientTimeout = 30000; //反弹连接超时时间（毫秒）
        public const int TokenExpiredMinutes = 60 * 24 * 30; //token过期时间（分钟）TODO 待生效

        public const string NSPClientServiceDisplayName = "NSPClient";//windows服务显示名
        public const string NSPClientServiceName = "NSPClient";//windows服务名

        public const int ClientUdpReceiveTimeout = 3000;//客户端udp接收超时时长（毫秒）

        public const int SslHandshakeTimeout = 15000;//SSL/TLS握手超时时间（毫秒），超时后直接断开连接

        /// <summary>
        /// 一个方向的传输结束后，留给另一个方向把在途数据写完的时间（毫秒）。
        /// 超过这个时间还没写完就直接拆连接。
        /// </summary>
        public const int TransferDrainTimeout = 5000;

        /// <summary>
        /// 单个转发帧的最大长度，用于约束按长度前缀读取时的内存分配，
        /// 防止畸形长度头导致一次性分配超大数组。
        /// </summary>
        public const int MaxTransferFrameSize = 16 * 1024 * 1024;

        public const int ClientTunnelBufferSize = 64 * 1024; //客户端数据包大小（从缓冲池租用）
        public const int ClientUdpBufferSize = 65535;//服务端udp数据包大小
        #region 服务端配置

        public const int StartArrangedPort = 20000;
        public const string NSPServerDisplayName = "NSPServer";//windows服务显示名
        public const string NSPServerServiceName = "NSPServer";//windows服务名

        public const int ServerTunnelBufferSize = 64 * 1024;//服务端数据包大小（从缓冲池租用）
        public const int ServerUdpBufferSize = 65535;//服务端udp数据包大小


        public static INSmartLogger Logger;
        #endregion
    }
}