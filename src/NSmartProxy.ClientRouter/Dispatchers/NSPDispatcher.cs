using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NSmartProxy.Data;
using NSmartProxy.Data.DTOs;

namespace NSmartProxy.ClientRouter.Dispatchers
{
    public class NSPDispatcher
    {
        private string BaseUrl;
        //TODO httpclient的一种解决方案：定时对象
        private static HttpClient _client;
        private static Timer _timer = new Timer(obj =>
        {
            _client?.Dispose();
            _client = null;
        });

        //_client.Dispose();_client = null

        public NSPDispatcher(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        public static HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    //_timer = new
                    _client = new HttpClient();
                }
                return _client;
            }
        }

        public async Task<HttpResult<LoginFormClientResult>> LoginFromClient(string username, string userpwd)
        {
            string url = $"http://{BaseUrl}/LoginFromClient";
            var httpmsg = await Client.GetAsync($"{url}?username={username}&userpwd={userpwd}").ConfigureAwait(false);
            var httpstr = await httpmsg.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<HttpResult<LoginFormClientResult>>(httpstr);
        }

        public async Task<HttpResult<LoginFormClientResult>> Login(string userid, string userpwd)
        {
            string url = $"http://{BaseUrl}/LoginFromClientById";
            var httpmsg = await Client.GetAsync($"{url}?username={userid}&userpwd={userpwd}").ConfigureAwait(false);
            var httpstr = await httpmsg.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<HttpResult<LoginFormClientResult>>(httpstr);
        }

        //TODO 增加一个校验用户token是否合法的方法
        //public 
        //GetServerPorts
        public async Task<HttpResult<ServerPortsDTO>> GetServerPorts()
        {
            string url = $"http://{BaseUrl}/GetServerPorts";
            var httpmsg = await Client.GetAsync(url).ConfigureAwait(false);
            var httpstr = await httpmsg.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<HttpResult<ServerPortsDTO>>(httpstr);
        }

        /// <summary>
        /// 当允许服务端端修改客户端时，从服务端获取配置
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResult<NSPClientConfig>> GetServerClientConfig(string token)
        {
            string url = $"http://{BaseUrl}/GetServerClientConfig";

            CookieContainer cookieContainer = new CookieContainer();
            Cookie cookie = new Cookie("NSPTK", token);
            cookie.Domain = BaseUrl.Substring(0, BaseUrl.IndexOf(':'));

            cookieContainer.Add(cookie);   // 加入Cookie
            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer,
                AllowAutoRedirect = true,
                UseCookies = true
            };

            HttpClient cookieClient = new HttpClient(httpClientHandler);
            var httpmsg = await cookieClient.GetAsync($"{url}?userid=").ConfigureAwait(false);
            var httpstr = await httpmsg.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<HttpResult<NSPClientConfig>>(httpstr);
        }
    }
}
