using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Clients
{
    public class ShopifyGraphQlClient
    {
        private readonly HttpClient _http;

        public ShopifyGraphQlClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<JObject> ExecuteAsync(
            string query,
            CancellationToken ct)
        {
            var payload = new
            {
                query
            };

            var resp = await _http.PostAsync(
                "/admin/api/2024-04/graphql.json",
                new StringContent(
                    JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json"),
                ct);

            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync(ct);
            return JObject.Parse(json);
        }
    }

}
