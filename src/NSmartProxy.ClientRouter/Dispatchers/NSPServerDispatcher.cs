using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NSmartProxy.ClientRouter.Dispatchers
{
    public class NSPServerDispatcher
    {

        public async Task<string> LoginFromClient(string username, string userpwd)
        {
            string url = "http://localhost:12309/login";
            HttpClient client = new HttpClient();
            await client.GetAsync($"{url}/LoginFromClient?username={username}&userpwd={userpwd}");
            return "";
        }
    }
}
