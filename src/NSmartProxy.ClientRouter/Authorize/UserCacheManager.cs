using System;
using System.IO;
using NSmartProxy.Data.Models;

namespace NSmartProxy.Client.Authorize
{
    public class UserCacheManager
    {
        //private Router router;
        //private ClientUserCache clientUserCache;
        //private string cachePath;

        //public ClientUserCacheItem GetCurrentUserCache()
        //{
        //    return clientUserCache.TryGetValue(router.ClientConfig.ProviderAddress + ":" +
        //                                       router.ClientConfig.ProviderWebPort, out var userCache) ? userCache : null;
        //}

        //private UserCacheManager()
        //{
        //}

        /// <summary>
        /// 通过地址获取用户信息
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="cachePath"></param>
        /// <returns></returns>
        public static ClientUserCacheItem GetUserCacheFromEndpoint(string endpoint, string cachePath)
        {
            ClientUserCache userCache = GetClientUserCache(cachePath);
            if (userCache.ContainsKey(endpoint))
            {
                return userCache[endpoint];
            }
            else
            {
                return null;
            }

        }

        ///// <summary>
        ///// 初始化
        ///// </summary>
        ///// <param name="pRouter"></param>
        ///// <param name="cachePath"></param>
        ///// <returns></returns>
        //public static UserCacheManager Init(Router pRouter, string cachePath)
        //{
        //    ClientUserCache userCache = GetClientUserCache(cachePath);
        //    var userCacheManager = new UserCacheManager
        //    {
        //        router = pRouter,
        //        clientUserCache = userCache
        //    };
        //    return userCacheManager;
        //}

        /// <summary>
        /// 获取整个缓存集合
        /// </summary>
        /// <param name="cachePath"></param>
        /// <returns></returns>
        public static ClientUserCache GetClientUserCache(string cachePath)
        {
            ClientUserCache userCache;
            if (!File.Exists(cachePath))
            {
                File.Create(cachePath).Close();
            }

            try
            {
                userCache = File.ReadAllText(cachePath).ToObject<ClientUserCache>();
            }
            catch //(Exception e)
            {
                // Console.WriteLine(e);
                userCache = null;
            }

            if (userCache == null)
            {
                userCache = new ClientUserCache();
            }

            return userCache;
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="cachePath"></param>
        /// <param name="clientUserCache"></param>
        public static void SaveChanges(string cachePath, ClientUserCache clientUserCache)
        {
            File.WriteAllText(cachePath, clientUserCache.ToJsonString());
        }

        /// <summary>
        /// 更新特定的用户
        /// </summary>
        /// <param name="token"></param>
        /// <param name="userName"></param>
        /// <param name="serverEndPoint"></param>
        /// <param name="cachePath"></param>
        public static void UpdateUser(string token, string userName, string serverEndPoint, string cachePath)
        {
            var clientUserCache = GetClientUserCache(cachePath);
            if (!clientUserCache.ContainsKey(serverEndPoint))
            {
                clientUserCache[serverEndPoint] = new ClientUserCacheItem();
            }

            var item = clientUserCache[serverEndPoint];
            item.Token = token;
            item.UserName = userName;
            SaveChanges(cachePath, clientUserCache);
        }
    }
}