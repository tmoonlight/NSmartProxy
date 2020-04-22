using System;
using System.Collections.Generic;

namespace NSmartProxy.Data
{
    /// <summary>
    /// 客户端，包含一个客户端的信息,传输用
    /// </summary>
    public class ClientModel: ByteSerializeableObject
    {
        public int ClientId;        //2
        public List<App> AppList;   //3 * N
        public string IP;           //no serialize
        public override byte[] ToBytes()
        {
            byte[] bytes = new byte[2 + AppList.Count * 3];
            byte[] clientIdBytes = IntTo2Bytes(ClientId);
            List<Byte> listBytes = new List<byte>();
            listBytes.Add(clientIdBytes[0]);
            listBytes.Add(clientIdBytes[1]);
            foreach (var app in AppList)
            {
                listBytes.Add((byte)app.AppId);
                listBytes.AddRange(IntTo2Bytes(app.Port));
            }
            return listBytes.ToArray();
        }

        public static ClientModel GetFromBytes(byte[] bytes, int totalLength = 0)
        {
            if (totalLength == 0)
            {
                totalLength = bytes.Length;
            }
            ClientModel client = new ClientModel();
            client.ClientId = (bytes[0] << 8) + bytes[1];
            client.AppList = new List<App>();
            int appCount = (totalLength - 2) / 3;
            if (((totalLength - 2) % 3) > 0)
            {
                throw new Exception("格式错误：获取客户端对象失败");
            }
            for (int i = 0; i < appCount; i++)
            {
                App app = new App()
                {
                    AppId = bytes[2 + 3 * i],
                    Port = (bytes[3 + 3 * i] << 8) + bytes[4 + 3 * i]
                };
                client.AppList.Add(app);
            }
            return client;
        }
    }
}