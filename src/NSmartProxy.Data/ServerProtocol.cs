namespace NSmartProxy.Data
{
    public enum ServerProtocol : byte
    {
        Heartbeat = 0x01,
        ClientNewAppRequest = 0x02,
        Reconnect = 0x03,
        CloseClient = 0x04
    }

    /// <summary>
    /// 反弹控制端口协议头
    /// </summary>
    public enum ControlMethod : byte
    {
        TCPTransfer = 0x01, 
        KeepAlive = 0x03,
        UDPTransfer = 0x04,
        // Control = 0x05, //控制协议，用来让服务端控制客户端的配置
        Reconnect = 0x05, //重置协议，服务端发送此信号让客户端重新连接
        ForceClose = 0x6, //抢登则强制下线
    }

}
