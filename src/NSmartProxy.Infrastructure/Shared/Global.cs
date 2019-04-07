namespace NSmartProxy.Shared
{
    /// <summary>
    /// 放一些全局公用的靜態變量
    /// </summary>
    public sealed class Global
    {
        public const string LogFormat = "";
        public const int HeartbeatInterval = 30000;  //心跳间隔（毫秒）
        public const int HeartbeatCheckInterval = 60000;  //心跳检测间隔（毫秒）
        public const int DefaultConnectTimeout = 30000; //默认连接超时时间
        public const int DefaultWriteAckTimeout = 10000;//调用具备ack确认协议的等待时间
    }
}