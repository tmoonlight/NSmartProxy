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

        public const int ClientTunnelBufferSize = 81920; //客户端数据包大小
        public const int ClientUdpBufferSize = 65535;//服务端udp数据包大小
        #region 服务端配置

        public const int StartArrangedPort = 20000;
        public const string NSPServerDisplayName = "NSPServer";//windows服务显示名
        public const string NSPServerServiceName = "NSPServer";//windows服务名

        public const int ServerTunnelBufferSize = 81920;//服务端数据包大小
        public const int ServerUdpBufferSize = 65535;//服务端udp数据包大小

        #endregion
    }
}