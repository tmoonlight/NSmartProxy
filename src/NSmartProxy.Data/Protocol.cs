namespace NSmartProxy.Data
{
    public enum Protocol : byte
    {
        Heartbeat = 0x01,
        ClientNewAppRequest = 0x02,
        ModifyApp = 0x03,
        CloseClient = 0x04
    }
}
