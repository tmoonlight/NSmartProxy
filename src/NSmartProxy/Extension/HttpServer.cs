using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSmartProxy.Interfaces;

namespace NSmartProxy.Extension
{
    class HttpServer
    {
        #region HTTPServer

        public INSmartLogger Logger;

        public HttpServer(INSmartLogger logger)
        {
            Logger = logger;
        }

        public async Task StartHttpService(CancellationTokenSource ctsHttp, int WebManagementPort)
        {
            try
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add($"http://+:{WebManagementPort}/");
                //TcpListener listenerConfigService = new TcpListener(IPAddress.Any, WebManagementPort);
                Logger.Debug("Listening HTTP request on port " + WebManagementPort.ToString() + "...");
                await AcceptHttpRequest(listener, ctsHttp);
            }
            catch (HttpListenerException ex)
            {
                Logger.Debug("Please run this program in administrator mode." + ex);
                Server.Logger.Error(ex.ToString(), ex);
            }
            catch (Exception ex)
            {
                Logger.Debug(ex);
                Server.Logger.Error(ex.ToString(), ex);
            }
        }

        private async Task AcceptHttpRequest(HttpListener httpService, CancellationTokenSource ctsHttp)
        {
            httpService.Start();
            while (true)
            {
                var client = await httpService.GetContextAsync();
                ProcessHttpRequestAsync(client);
            }
        }

        private async Task ProcessHttpRequestAsync(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;

                response.ContentEncoding = Encoding.UTF8;
                response.ContentType = "text/html;charset=utf-8";

                //TODO ***通过request来的值进行接口调用

                //getJson
                var json = GetClientsInfoJson();
                await response.OutputStream.WriteAsync(HtmlUtil.GetContent(json.ToString()));
                //await response.OutputStream.WriteAsync(HtmlUtil.GetContent(request.RawUrl));
                response.OutputStream.Close();
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                throw;
            }
        }


        private string GetClientsInfoJson()
        {
            var ConnectionManager = ClientConnectionManager.GetInstance();
            StringBuilder json = new StringBuilder("[ ");
            foreach (var app in ConnectionManager.PortAppMap)
            {
                json.Append("{ ");
                json.Append(KV2Json("port", app.Key)).C();
                json.Append(KV2Json("clientId", app.Value.ClientId)).C();
                json.Append(KV2Json("appId", app.Value.AppId)).C();
                json.Append(KV2Json("blocksCount", app.Value.TcpClientBlocks.Count)).C();
                //反向连接
                json.Append(KV2Json("revconns"));
                json.Append("[ ");
                foreach (var reverseClient in app.Value.ReverseClients)
                {
                    json.Append("{ ");
                    if (reverseClient.Connected)
                    {
                        json.Append(KV2Json("lEndPoint", reverseClient.Client.LocalEndPoint.ToString())).C();
                        json.Append(KV2Json("rEndPoint", reverseClient.Client.RemoteEndPoint.ToString()));
                    }

                    //json.Append(KV2Json("p", c)).C();
                    //json.Append(KV2Json("port", ca.Key));
                    json.Append("}");
                    json.C();
                }

                json.D();
                json.Append("]").C();
                ;

                //隧道状态
                json.Append(KV2Json("tunnels"));
                json.Append("[ ");
                foreach (var tunnel in app.Value.Tunnels)
                {
                    json.Append("{ ");
                    if (tunnel.ClientServerClient != null)
                    {
                        Socket sktClient = tunnel.ClientServerClient.Client;
                        if (tunnel.ClientServerClient.Connected)

                            json.Append(KV2Json("clientServerClient", $"{sktClient.LocalEndPoint}-{sktClient.RemoteEndPoint}"))
                                .C();
                    }
                    if (tunnel.ConsumerClient != null)
                    {
                        Socket sktConsumer = tunnel.ConsumerClient.Client;
                        if (tunnel.ConsumerClient.Connected)
                            json.Append(KV2Json("consumerClient", $"{sktConsumer.LocalEndPoint}-{sktConsumer.RemoteEndPoint}"))
                                .C();
                    }

                    json.D();
                    //json.Append(KV2Json("p", c)).C();
                    //json.Append(KV2Json("port", ca.Key));
                    json.Append("}");
                    json.C();
                }

                json.D();
                json.Append("]");
                json.Append("}").C();
            }

            json.D();
            json.Append("]");
            return json.ToString();
        }

        private string KV2Json(string key)
        {
            return "\"" + key + "\":";
        }
        private string KV2Json(string key, object value)
        {
            return "\"" + key + "\":\"" + value.ToString() + "\"";
        }

        #endregion
        //TODO XXXX
        //API login

        //API Users
        //REST
        //AddUser
        //RemoveUser
        //
        
        //NoApi Auth
    }
}
