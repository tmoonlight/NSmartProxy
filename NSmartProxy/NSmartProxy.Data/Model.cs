using System;
using System.Collections.Generic;

namespace NSmartProxy.Data
{
    public class Client
    {
        public int ClientId;        //2
        public List<App> AppList;   //3 * N
        public byte[] ToBytes()
        {
            byte[] bytes = new byte[2 + AppList.Count * 3];
            byte[] clientIdBytes = StringUtil.IntTo2Bytes(ClientId);
            List<Byte> listBytes = new List<byte>();
            listBytes.Add(clientIdBytes[0]);
            listBytes.Add(clientIdBytes[1]);
            foreach (var app in AppList)
            {
                listBytes.Add((byte)app.AppId);
                listBytes.AddRange(StringUtil.IntTo2Bytes(app.Port));
            }
            return listBytes.ToArray();
        }

        public static Client GetFromBytes(byte[] bytes)
        {
            Client client = new Client();
            client.ClientId = bytes[0] << 8 + bytes[1];
            client.AppList = new List<App>();
            int appCount = (bytes.Length - 2) / 3;
            if (((bytes.Length - 2) % 3) > 0)
            {
                throw new Exception("error format");
            }
            for (int i = 2; i < appCount; i++)
            {
                App app = new App()
                {
                    AppId = bytes[2 + 3 * i],
                    Port = bytes[3 + 3 * i] << 8 + bytes[4 + 3 * i]
                };
                client.AppList.Add(app);
            }
            return client;
        }
    }

    public class App
    {
        //id需要大于1，否则会有很多问题
        public int AppId;           //1
        public int Port;            //2
    }

    public class ClientIdAppId
    {
        public int ClientId;        //2
        public int AppId;           //1

    }
}
