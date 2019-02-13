using System;
using System.Collections.Generic;
using System.Linq;

namespace NSmartProxy.Data
{
    /// <summary>
    /// 客户端，包含一个客户端的信息
    /// </summary>
    public class ClientModel
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
                throw new Exception("error format");
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

    /// <summary>
    /// 一个App
    /// </summary>
    public class App
    {
        //id需要大于1，否则会有很多问题
        public int AppId;           //1
        public int Port;            //2
    }

    /// <summary>
    /// 客户端和appid的组合
    /// </summary>
    public class ClientIdAppId
    {
        public int ClientId;        //2
        public int AppId;           //1
        public byte[] ToBytes()
        {
            byte[] bytes = new byte[3];
            byte[] clientIdBytres = StringUtil.IntTo2Bytes(ClientId);
            bytes[0] = clientIdBytres[0];
            bytes[1] = clientIdBytres[1];
            bytes[2] = (byte)AppId;
            return bytes;
        }

        public static ClientIdAppId GetFromBytes(byte[] bytes)
        {
            return new ClientIdAppId
            {
                ClientId = StringUtil.DoubleBytesToInt(bytes[0], bytes[1]),
                AppId = bytes[2]
            };
        }
    }


    //public class ClientApp
    //{
    //    public int ClientId;
    //    public int AppId;
    //    public int TargetServicePort;
    //}

    /// <summary>
    /// 客户端向服务端申请新app的请求包
    /// </summary>
    public class ClientNewAppRequest
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

    public class ClientApp
    {
        public int AppId;
        public string IP;
        public int TargetServicePort;
    }
}
