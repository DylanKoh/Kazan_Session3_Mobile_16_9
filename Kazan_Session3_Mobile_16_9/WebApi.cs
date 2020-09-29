using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kazan_Session3_Mobile_16_9
{
    public class WebApi
    {
        string baseAddress = "http://10.0.2.2/";

        public async Task<string> PostAsync(string data, string extSite)
        {
            var client = new HttpClient();
            var response = "";
            var site = baseAddress + extSite;
            if (data == null)
            {
                var emptyContent = new StringContent("", Encoding.UTF8, "application/json");
                response = await client.PostAsync(site, emptyContent).Result.Content.ReadAsStringAsync();
            }
            else
            {
                var content = new StringContent(data, Encoding.UTF8, "application/json");
                response = await client.PostAsync(site, content).Result.Content.ReadAsStringAsync();
            }
            return response;
        }
    }
}
