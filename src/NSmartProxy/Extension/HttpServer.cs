using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSmartProxy.Database;
using NSmartProxy.Interfaces;

namespace NSmartProxy.Extension
{
    partial class HttpServer
    {
        #region HTTPServer

        public INSmartLogger Logger;
        public IDbOperator Dbop;

        public HttpServer(INSmartLogger logger, IDbOperator dbop)
        {
            Logger = logger;
            Dbop = dbop;
            //第一次加载所有mime类型
            PopulateMappings();

        }

        public async Task StartHttpService(CancellationTokenSource ctsHttp, int WebManagementPort)
        {
            try
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add($"http://*:{WebManagementPort}/");
                listener.Prefixes.Add($"http://2017studio.imwork.net:{WebManagementPort}/");
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
            Logger.Debug("Http服务结束。");
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
            string baseFilePath = "./Extension/HttpServerStaticFiles/";
            var request = context.Request;
            var response = context.Response;
            //TODO XX 设置该同源策略为了方便调试，请确保web项目也位于locahost5671上
            response.AddHeader("Access-Control-Allow-Origin", "http://localhost:5671");

            try
            {
                //TODO ***通过request来的值进行接口调用
                string unit = request.RawUrl.Replace("//", "");
                int idx1 = unit.LastIndexOf("#");
                if (idx1 > 0) unit = unit.Substring(0, idx1);
                int idx2 = unit.LastIndexOf("?");
                if (idx2 > 0) unit = unit.Substring(0, idx2);
                int idx3 = unit.LastIndexOf(".");

                //TODO 通过后缀获取不同的文件，若无后缀，则调用接口
                if (idx3 > 0)
                {

                    if (!File.Exists(baseFilePath + unit))
                    {
                        Server.Logger.Debug($"未找到文件{baseFilePath + unit}");
                        return;

                    }
                    //mime类型
                    ProcessMIME(response, unit.Substring(idx3));
                    using (FileStream fs = new FileStream(baseFilePath + unit, FileMode.Open))
                    {
                        await fs.CopyToAsync(response.OutputStream);
                    }
                }
                else
                {
                    unit = unit.Replace("/", "");
                    response.ContentEncoding = Encoding.UTF8;
                    response.ContentType = "application/json";

                    //TODO XXXXXX 调用接口 接下来要用分布类隔离并且用API特性限定安全
                    object jsonObj;
                    //List<string> qsStrList;
                    int qsCount = request.QueryString.Count;
                    object[] parameters = null;
                    if (qsCount > 0)
                    {
                        parameters = new object[request.QueryString.Count];
                        for (int i = 0; i < request.QueryString.Count; i++)
                        {
                            parameters[i] = request.QueryString[i];
                        }
                    }

                    // request.QueryString[0]
                    try
                    {
                        jsonObj = this.GetType().GetMethod(unit).Invoke(this, parameters);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.Message, e);
                        jsonObj = e.Message + "---" + e.StackTrace;
                    }

                    //getJson
                    //var json = GetClientsInfoJson();

                    await response.OutputStream.WriteAsync(HtmlUtil.GetContent("invoke错误:" + jsonObj.ToJsonString()));
                    //await response.OutputStream.WriteAsync(HtmlUtil.GetContent(request.RawUrl));
                    // response.OutputStream.Close();

                }
                //suffix = unit.Substring(unit.LastIndexOf(".")+1,)

            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                throw;
            }
            finally
            {
                response.OutputStream.Close();
            }
        }

        private void ProcessMIME(HttpListenerResponse response, string suffix)
        {
            if (suffix == ".html" || suffix == ".js")
            {
                response.ContentEncoding = Encoding.UTF8;
            }

            string val = "";
            if (_mimeMappings.TryGetValue(suffix, out val))
            {
                // found!
                response.ContentType = val;
            }
            else
            {
                response.ContentType = "application/octet-stream";
            }

        }

        #endregion

    }
}
