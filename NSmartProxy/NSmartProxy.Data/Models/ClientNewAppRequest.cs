namespace NSmartProxy.Data
{
    /// <summary>
    /// 客户端向服务端申请新app的请求包
    /// </summary>
    public class ClientNewAppRequest : IByteSerializeable
    {
        public int ClientId;    //2
        public int ClientCount; //1
        public byte[] ToBytes()
        {
            byte[] bytes = new byte[3];
            byte[] clientIdBytres = StringUtil.IntTo2Bytes(ClientId);
            bytes[0] = clientIdBytres[0];
            bytes[2] = (byte)ClientCount;
            return bytes;
        }
        public static ClientNewAppRequest GetFromBytes(byte[] bytes)
        {
            return new ClientNewAppRequest
            {
                ClientId = StringUtil.DoubleBytesToInt(bytes[0], bytes[1]),
                ClientCount = bytes[2]
            };
        }
    }
}