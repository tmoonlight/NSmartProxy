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
       
    }

}
