using Dekofar.HyperConnect.Integrations.Shopify.Common;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Clients
{
    public class ShopifyRestClient
    {
        private readonly HttpClient _http;
        private readonly ShopifyOptions _opt;

        public ShopifyRestClient(HttpClient http, IOptions<ShopifyOptions> options)
        {
            _http = http;
            _opt = options.Value;

            _http.BaseAddress = new Uri(_opt.BaseUrl);
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("X-Shopify-Access-Token", _opt.AccessToken);
            _http.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // 🔵 Günlük operasyonlar
        public async Task<List<Order>> GetOrdersByQueryAsync(
            string query,
            CancellationToken ct)
        {
            var url =
                $"/admin/api/2024-04/orders.json?limit=250&status=any&query={Uri.EscapeDataString(query)}";

            var resp = await _http.GetAsync(url, ct);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync(ct);
            var parsed = JsonConvert.DeserializeObject<ShopifyOrdersResponse>(json);

            return parsed?.Orders ?? new();
        }
    }

}
