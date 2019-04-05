namespace NSmartProxy.Data
{
    /// <summary>
    /// 客户端和appid的组合
    /// </summary>
    public class ClientIdAppId : ByteSerializeableObject
    {
        public int ClientId;        //2
        public int AppId;           //1
        public override byte[] ToBytes()
        {
            byte[] bytes = new byte[3];
            byte[] clientIdBytres = IntTo2Bytes(ClientId);
            bytes[0] = clientIdBytres[0];
            bytes[1] = clientIdBytres[1];
            bytes[2] = (byte)AppId;
            return bytes;
        }

        public static ClientIdAppId GetFromBytes(byte[] bytes)
        {
            return new ClientIdAppId
            {
                ClientId = DoubleBytesToInt(bytes[0], bytes[1]),
                AppId = bytes[2]
            };
        }
    }
}