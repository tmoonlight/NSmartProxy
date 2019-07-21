namespace NSmartProxy.Shared
{
    /// <summary>
    /// 放一些全局公用的静态变量
    /// </summary>
    public sealed class Global
    {
        public const string NO_TOKEN_STRING = "notoken";
        public const string NSmartProxyClientName = "NSmartProxy Client v0.9";
        public const string NSmartProxyServerName = "NSmartProxy Server v0.9";

        public const int ClientReconnectInterval = 3000;//客户端断线重连时间间隔（毫秒）

        public const int HeartbeatInterval = 30000;  //心跳间隔（毫秒）
        public const int HeartbeatCheckInterval = 90000;  //心跳检测间隔（毫秒）
        public const int DefaultConnectTimeout = 30000; //默认连接超时时间
        public const int DefaultWriteAckTimeout = 10000;//调用具备ack确认协议的等待时间
        public const int DefaultPopClientTimeout = 30000; //反弹连接超时时间
        public const int TokenExpiredMinutes = 60 * 24 * 30; //token过期时间（分钟）TODO 待生效

        public const string DisplayName = "NSPClient";//windows服务显示名
        public const string ServiceName = "NSPClient";//windows服务名

        #region 服务端配置

        public const int StartArrangedPort = 20000;

        #endregion
    }
}