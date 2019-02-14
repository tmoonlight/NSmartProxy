using NSmartProxy.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSmartProxy.Client
{
    public class ClientRouter
    {
        // public const string TARGET_SERVICE_ADDRESS = "127.0.0.1";
        //  public const int TARGET_SERVICE_ADDRESS_PORT = 80;
        public const string PROVIDER_ADDRESS = "192.168.1.2";//<-important 服务器ip
        public const int PROVIDER_ADDRESS_PORT = 9973;// static
        public const int PROVIDER_CONFIG_SERVICE_PORT = 12307; //static

        public string TargetServices = "127.0.0.1:80,127.0.0.1:3389,127.0.0.1:21";
        CancellationTokenSource CANCELTOKEN = new CancellationTokenSource();
        CancellationTokenSource TRANSFERING_TOKEN = new CancellationTokenSource();
        ServerConnnectionManager ConnnectionManager;
        //public Dictionary<int, int> AppPortMap;  //key:appid,value:servicetargetport 目标端口
        public static List<ClientApp> appIdIpPortConfig = new List<ClientApp>(); // app编号，ip地址，目标服务端口

        /// <summary>
        /// 重要：连接服务端
        /// </summary>
        /// <returns></returns>
        public async Task ConnectToProvider()
        {
            appIdIpPortConfig = new List<ClientApp>() {
                new ClientApp(){AppId = 0, IP = "127.0.0.1", TargetServicePort = 10001 },
                new ClientApp(){AppId = 0, IP = "192.168.1.2", TargetServicePort = 10002 },
                new ClientApp(){AppId = 0, IP = "127.0.0.1", TargetServicePort = 10003 }
            };


            ConnnectionManager = ServerConnnectionManager.GetInstance();
            ConnnectionManager.ClientGroupConnected += ServerConnnectionManager_ClientGroupConnected;
            var clientModel = ConnnectionManager.InitConfig();
            //foreach (var kv in ConnnectionManager.ServiceClientListCollection)
            //{
            //    ClientAppWorker caw = kv.Value;
            //    appIdIpPortConfig.First((obj) => obj.TargetServicePort == caw.Port).AppId = caw.AppId;
            //}
            int counter = 0;
            //appid为0时说明没有分配appid，所以需要分配一个
            foreach (var app in appIdIpPortConfig)
            {
                if (app.AppId == 0)
                {
                    app.AppId = clientModel.AppList[counter].AppId;
                    counter++;
                }
            }
            Console.WriteLine("**********port list*******");
            
            foreach (var ap in clientModel.AppList)
            {
                //Console.WriteLine("*                        *");
                //Console.SetCursorPosition(1)
                Console.WriteLine(ap.AppId.ToString() + ":" + ap.Port.ToString());
            }
            Console.WriteLine("**************************");
            await ConnnectionManager.PollingToProvider();
        }

        private void ServerConnnectionManager_ClientGroupConnected(object sender, EventArgs e)
        {
            var args = (ClientGroupEventArgs)e;
            foreach (TcpClient providerClient in args.NewClients)
            {

                Console.WriteLine("开启连接");
                OpenTrasferation(args.App.AppId, providerClient);
            }

        }

        private async Task OpenTrasferation(int appId, TcpClient providerClient)
        {
            byte[] buffer = new byte[4096];
            NetworkStream providerClientStream = providerClient.GetStream();
            //接收首条消息，首条消息中返回的是appid和客户端
            int readByteCount = await providerClientStream.ReadAsync(buffer);
            //从空闲连接列表中移除
            ConnnectionManager.RemoveClient(appId, providerClient);
            Console.WriteLine(appId + "接受到首条信息");
            TcpClient toTargetServer = new TcpClient();
            //根据clientid_appid发送到固定的端口
            ClientApp item = appIdIpPortConfig.First((obj) => obj.AppId == appId);
            // item1:app编号，item2:ip地址，item3:目标服务端口
            toTargetServer.Connect(item.IP, item.TargetServicePort);
            NetworkStream targetServerStream = toTargetServer.GetStream();
            targetServerStream.Write(buffer, 0, readByteCount);
            await TcpTransferAsync(providerClientStream, targetServerStream);
            //close connection
            providerClient.Close();
            Console.WriteLine("关闭一条连接");
        }


        private async Task TcpTransferAsync(NetworkStream providerStream, NetworkStream targetServceStream)
        {
            Console.WriteLine("Looping start.");
            //创建相互转发流
            var taskT2PLooping = ToStaticTransfer(TRANSFERING_TOKEN.Token, targetServceStream, providerStream, "T2P");
            var taskP2TLooping = StreamTransfer(TRANSFERING_TOKEN.Token, providerStream, targetServceStream, "P2T");


            //close connnection,whether client or server stopped transferring.
            var comletedTask = await Task.WhenAny(taskT2PLooping, taskP2TLooping);
            Console.WriteLine(comletedTask.Result + "传输关闭，重新读取字节");
        }



        private async Task<string> StreamTransfer(CancellationToken ct, NetworkStream fromStream, NetworkStream toStream, string signal, Func<byte[], Task<bool>> beforeTransfer = null)
        {
            await fromStream.CopyToAsync(toStream, ct);
            return signal;
        }


        private async Task<string> ToStaticTransfer(CancellationToken ct, NetworkStream fromStream, NetworkStream toStream, string signal, Func<byte[], Task<bool>> beforeTransfer = null)
        {

            await fromStream.CopyToAsync(toStream, ct);
            return signal;
        }

        private void SendZero(int port)
        {
            TcpClient tc = new TcpClient();
            tc.Connect("127.0.0.1", port);
            tc.Client.Send(new byte[] { 0 });
        }
    }

}
