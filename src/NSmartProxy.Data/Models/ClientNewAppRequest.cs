using System.Collections.Generic;

namespace NSmartProxy.Data
{
    /// <summary>
    /// 客户端向服务端申请新app的请求包
    /// </summary>
    public class ClientNewAppRequest : ByteSerializeableObject
    {
        public int TokenLength; //2
        public string Token;    //TokenLength
        public int ClientId;    //2
        public int ClientCount; //1
        public override byte[] ToBytes()
        {
            //byte[] bytes = new byte[3];
            //byte[] clientIdBytres = IntTo2Bytes(ClientId);
            //bytes[0] = clientIdBytres[0];
            //bytes[2] = (byte)ClientCount;
            List<byte> Allbytes = new List<byte>(30);
            byte[] bytes1 = System.Text.ASCIIEncoding.ASCII.GetBytes(Token);
            byte[] bytes0 = IntTo2Bytes(bytes1.Length);
            byte[] bytes2 = IntTo2Bytes(ClientId);
            byte bytes3 = (byte)ClientCount;
            Allbytes.AddRange(bytes0);
            Allbytes.AddRange(bytes1);
            Allbytes.AddRange(bytes2);
            Allbytes.Add(bytes3);
            return Allbytes.ToArray();
        }

        //public static ClientNewAppRequest GetFromBytes(byte[] bytes)
        //{
        //    return new ClientNewAppRequest
        //    {
        //        ClientId = DoubleBytesToInt(bytes[0], bytes[1]),
        //        ClientCount = bytes[2]
        //    };
        //}
    }
}