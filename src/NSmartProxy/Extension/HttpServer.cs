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
using NSmartProxy.Authorize;
using NSmartProxy.Data;
using NSmartProxy.Data.DTOs;
using NSmartProxy.Database;
using NSmartProxy.Infrastructure;
using NSmartProxy.Interfaces;
// ReSharper disable All

namespace NSmartProxy.Extension
{
    partial class HttpServer
    {
        #region HTTPServer

        public INSmartLogger Logger;
        public IDbOperator Dbop;

        private const string INDEX_PAGE = "/main.html";
        private const string BASE_FILE_PATH = "./Extension/HttpServerStaticFiles/";
        private const string BASE_LOG_FILE_PATH = "./log";

        public Dictionary<string, MemoryStream> FilesCache = new Dictionary<string, MemoryStream>(20);
        public NSPServerContext ServerContext { get; }

        public HttpServer(INSmartLogger logger, IDbOperator dbop, NSPServerContext serverContext)
        {
            Logger = logger;
            Dbop = dbop;
            //第一次加载所有mime类型
            PopulateMappings();
            ServerContext = serverContext;
        }



        /// <summary>
        /// http服务启动，初始化代码写在这里
        /// </summary>
        /// <param name="ctsHttp"></param>
        /// <param name="WebManagementPort"></param>
        /// <returns></returns>
        public async Task StartHttpService(CancellationTokenSource ctsHttp, int WebManagementPort)
        {
            try
            {
                HttpListener listener = new HttpListener();
                //缓存所有文件
                var dir = new DirectoryInfo(BASE_FILE_PATH);
                var files = dir.GetFiles("*.*");
                foreach (var file in files)
                {
                    using (var fs = file.OpenRead())
                    {
                        var mms = new MemoryStream();
                        fs.CopyTo(mms);
                        FilesCache.Add(file.Name, mms);
                    }
                }
                Logger.Debug($"{files.Length} files cached.");

                //如果库中没有任何记录，则增加默认用户
                if (Dbop.GetLength() < 1)
                {
                    AddUserV2("admin", "admin", "1");
                }

                listener.Prefixes.Add($"http://+:{WebManagementPort}/");
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
                _ = ProcessHttpRequestAsync(client);
            }
        }

        private async Task ProcessHttpRequestAsync(HttpListenerContext context)
        {

            var request = context.Request;
            var response = context.Response;
            //TODO XX 设置该同源策略为了方便调试，真实项目可以确保同源

#if DEBUG
            response.AddHeader("Access-Control-Allow-Origin", "*");
#endif

            try
            {
                //通过request来的值进行接口调用
                string unit = request.RawUrl.Replace("//", "");

                if (unit == "/") unit = INDEX_PAGE;

                int idx1 = unit.LastIndexOf("#");
                if (idx1 > 0) unit = unit.Substring(0, idx1);
                int idx2 = unit.LastIndexOf("?");
                if (idx2 > 0) unit = unit.Substring(0, idx2);
                int idx3 = unit.LastIndexOf(".");

                //通过后缀获取不同的文件，若无后缀，则调用接口
                if (idx3 > 0)
                {

                    if (!File.Exists(BASE_FILE_PATH + unit))
                    {
                        Server.Logger.Debug($"未找到文件{BASE_FILE_PATH + unit}");
                        return;

                    }
                    //mime类型
                    ProcessMIME(response, unit.Substring(idx3));
                    //TODO 权限控制（只是控制html权限而已）

                    //读文件优先去缓存读
                    if (FilesCache.TryGetValue(unit.TrimStart('/'), out MemoryStream memoryStream))
                    {
                        memoryStream.Position = 0;
                        await memoryStream.CopyToAsync(response.OutputStream);
                    }
                    else
                    {
                        using (FileStream fs = new FileStream(BASE_FILE_PATH + unit, FileMode.Open))
                        {
                            await fs.CopyToAsync(response.OutputStream);
                        }
                    }
                }
                else  //url中没有小数点则是接口
                {
                    unit = unit.Replace("/", "");
                    response.ContentEncoding = Encoding.UTF8;

                    //调用接口 用分布类隔离并且用API特性限定安全
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

                    //反射调用API方法
                    MethodInfo method = null;
                    try
                    {
                        method = this.GetType().GetMethod(unit);
                        if (method == null)
                        {
                            //Server.Logger.Debug($"无效的方法名{unit}");
                            throw new Exception($"无效的方法名{unit}");
                        }

                        //TODO 先不校验，方便调试
#if !DEBUG
                        if (method.GetCustomAttribute<SecureAttribute>() != null)
                        {
                            if (request.Cookies["NSPTK"] == null)
                                throw new Exception("用户未登录。");
                            //TODO cookie，根据不同的用户角色分配权限。
                            var UserClaims = StringUtil.ConvertStringToTokenClaims(request.Cookies["NSPTK"].Value);
                            if (string.IsNullOrEmpty(UserClaims.UserKey))
                            {
                                throw new Exception("登录信息异常。");
                            }
                        }
#endif

                        if (method.GetCustomAttribute<APIAttribute>() != null)
                        {
                            response.ContentType = "application/json";
                            jsonObj = method.Invoke(this, parameters);
                            await response.OutputStream.WriteAsync(HtmlUtil.GetContent(jsonObj.Wrap().ToJsonString()));
                        }
                        else if (method.GetCustomAttribute<FormAPIAttribute>() != null)
                        {
                            response.ContentType = "text/html";
                            jsonObj = method.Invoke(this, parameters);
                            await response.OutputStream.WriteAsync(HtmlUtil.GetContent(jsonObj.ToString()));
                        }
                        else if (method.GetCustomAttribute<ValidateAPIAttribute>() != null)
                        {
                            response.ContentType = "application/json";
                            bool validateResult = (bool)method.Invoke(this, parameters);
                            if (validateResult == true)
                            {
                                await response.OutputStream.WriteAsync(HtmlUtil.GetContent("{\"valid\":true}"));
                            }
                            else
                            {
                                await response.OutputStream.WriteAsync(HtmlUtil.GetContent("{\"valid\":false}"));
                            }
                        }
                        else if (method.GetCustomAttribute<FileAPIAttribute>() != null)
                        {
                            response.ContentType = "application/octet-stream";
                            if (!(method.Invoke(this, parameters) is FileDTO fileDto))
                            {
                                throw new Exception("文件返回失败，请查看错误日志。");
                            }

                            response.Headers.Add("Content-Disposition", "attachment;filename=" + fileDto.FileName);
                            //response.OutputStream.(stream);
                            using (fileDto.FileStream)
                            {
                                await fileDto.FileStream.CopyToAsync(response.OutputStream);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message, ex);
                        jsonObj = new Exception(ex.Message + "---" + ex.StackTrace);
                        response.ContentType = "application/json";
                        await response.OutputStream.WriteAsync(HtmlUtil.GetContent(jsonObj.Wrap().ToJsonString()));
                    }
                    finally
                    {

                    }
                }

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

            if (_mimeMappings.TryGetValue(suffix, out string val))
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
