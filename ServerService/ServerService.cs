using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ServerService.Proxy;

namespace ServerService
{
    internal class ServerService : IServerService
    {
        static readonly HttpClient client = new HttpClient();
        ProxyServiceClient proxy = new ProxyServiceClient();

        public async Task<String> getJSON(String url)
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
    }
}
