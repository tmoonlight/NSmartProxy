using NSmartProxy.Data;
using NSmartProxy.Interfaces;
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
    public class Router
    {
        CancellationTokenSource CANCEL_TOKEN = new CancellationTokenSource();
        CancellationTokenSource TRANSFERING_TOKEN = new CancellationTokenSource();
        ServerConnnectionManager ConnnectionManager;

        internal static Config ClientConfig;

        //inject
        internal static INSmartLogger Logger;

        public Router(INSmartLogger logger)
        {
            Logger = logger;
        }

        public void SetConifiguration(Config config)
        {
            ClientConfig = config;
        }

        /// <summary>
        /// 重要：连接服务端
        /// </summary>
        /// <returns></returns>
        public async Task ConnectToProvider()
        {
            var appIdIpPortConfig = ClientConfig.Clients;

            //1.获取配置
            ConnnectionManager = ServerConnnectionManager.Create();
            ConnnectionManager.ClientGroupConnected += ServerConnnectionManager_ClientGroupConnected;
            var clientModel = await ConnnectionManager.InitConfig();
            int counter = 0;
            //2.分配配置：appid为0时说明没有分配appid，所以需要分配一个
            foreach (var app in appIdIpPortConfig)
            {
                if (app.AppId == 0)
                {
                    app.AppId = clientModel.AppList[counter].AppId;
                    counter++;
                }
            }
            Logger.Debug("****************port list*************");

            foreach (var ap in clientModel.AppList)
            {
                var cApp = appIdIpPortConfig.First(obj => obj.AppId == ap.AppId);
                Logger.Debug(ap.AppId.ToString() + ":  " + ClientConfig.ProviderAddress + ":" + ap.Port.ToString() + "=>" +
                     cApp.IP + ":" + cApp.TargetServicePort);
            }
            Logger.Debug("**************************************");
            Task pollingTask = ConnnectionManager.PollingToProvider();
            //3.创建心跳连接
            ConnnectionManager.StartHeartBeats(5000);

            try
            {
                await pollingTask;
            }
            catch (Exception ex)
            {
                Logger.Error("Thread:" + Thread.CurrentThread.ManagedThreadId + " crashed.\n", ex);
                throw;
            }

            await Task.Delay(TimeSpan.FromHours(24), CANCEL_TOKEN.Token);
        }

        private void ServerConnnectionManager_ClientGroupConnected(object sender, EventArgs e)
        {
            var args = (ClientGroupEventArgs)e;
            foreach (TcpClient providerClient in args.NewClients)
            {

                Router.Logger.Debug("Open server connection.");
                OpenTrasferation(args.App.AppId, providerClient);
            }

        }

        private async Task OpenTrasferation(int appId, TcpClient providerClient)
        {
            try
            {
                byte[] buffer = new byte[1];
                NetworkStream providerClientStream = providerClient.GetStream();
                //接收首条消息，首条消息中返回的是appid和客户端
                int readByteCount = await providerClientStream.ReadAsync(buffer, 0, buffer.Length);
                //从空闲连接列表中移除
                ConnnectionManager.RemoveClient(appId, providerClient);
                //每移除一个链接则发起一个新的链接
                Router.Logger.Debug(appId + "接收到连接请求");
                TcpClient toTargetServer = new TcpClient();
                //根据clientid_appid发送到固定的端口
                ClientApp item = ClientConfig.Clients.First((obj) => obj.AppId == appId);

                //只发送一次，需要在链接成功移除时加入
                await ConnnectionManager.ConnectAppToServer(appId);
                Router.Logger.Debug("已建立反向连接:" + appId);
                // item1:app编号，item2:ip地址，item3:目标服务端口
                toTargetServer.Connect(item.IP, item.TargetServicePort);
                Router.Logger.Debug("已连接目标服务:" + item.IP.ToString() + ":" + item.TargetServicePort.ToString());

                NetworkStream targetServerStream = toTargetServer.GetStream();
                //targetServerStream.Write(buffer, 0, readByteCount);
                TcpTransferAsync(providerClientStream, targetServerStream, providerClient, toTargetServer);
                //already close connection

            }
            catch (Exception e)
            {
                Logger.Debug(e);
                throw;
            }

        }


        private async Task TcpTransferAsync(NetworkStream providerStream, NetworkStream targetServceStream, TcpClient providerClient, TcpClient toTargetServer)
        {
            try
            {
                Router.Logger.Debug("Looping start.");
                //创建相互转发流
                var taskT2PLooping = ToStaticTransfer(TRANSFERING_TOKEN.Token, targetServceStream, providerStream, "T2P");
                var taskP2TLooping = StreamTransfer(TRANSFERING_TOKEN.Token, providerStream, targetServceStream, "P2T");

                //close connnection,whether client or server stopped transferring.
                var comletedTask = await Task.WhenAny(taskT2PLooping, taskP2TLooping);
                //Router.Logger.Debug(comletedTask.Result + "传输关闭，重新读取字节");
                providerClient.Close();
                Router.Logger.Debug("已关闭toProvider连接。");
                toTargetServer.Close();
                Router.Logger.Debug("已关闭toTargetServer连接。");
            }
            catch (Exception ex)
            {
                Router.Logger.Debug(ex.ToString());
                throw;
            }
        }



        private async Task<string> StreamTransfer(CancellationToken ct, NetworkStream fromStream, NetworkStream toStream, string signal, Func<byte[], Task<bool>> beforeTransfer = null)
        {
            await fromStream.CopyToAsync(toStream, 4096, ct);
            return signal;
        }


        private async Task<string> ToStaticTransfer(CancellationToken ct, NetworkStream fromStream, NetworkStream toStream, string signal, Func<byte[], Task<bool>> beforeTransfer = null)
        {

            await fromStream.CopyToAsync(toStream, 4096, ct);
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
