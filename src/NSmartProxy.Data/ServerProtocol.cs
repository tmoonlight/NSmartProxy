namespace NSmartProxy.Data
{
    public enum ServerProtocol : byte
    {
        Heartbeat = 0x01,
        ClientNewAppRequest = 0x02,
        Reconnect = 0x03,
        CloseClient = 0x04
    }

    public enum ControlMethod : byte
    {
        TCPTransfer = 0x01,
        UDPTransfer= 0x02,
        KeepAlive = 0x03
    }

}
