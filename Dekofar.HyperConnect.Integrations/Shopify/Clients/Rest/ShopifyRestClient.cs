using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models.Raw;
using System.Net.Http.Json;

namespace Dekofar.HyperConnect.Integrations.Shopify.Clients.Rest
{
    public class ShopifyRestClient
    {
        private readonly HttpClient _http;

        public ShopifyRestClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Order>> GetAllOrdersAsync(CancellationToken ct)
        {
            var allOrders = new List<Order>();
            string? nextPageUrl =
                "/admin/api/2024-04/orders.json" +
                "?limit=250" +
                "&status=any";

            while (!string.IsNullOrEmpty(nextPageUrl))
            {
                var response = await _http.GetAsync(nextPageUrl, ct);
                response.EnsureSuccessStatusCode();

                var data = await response.Content
                    .ReadFromJsonAsync<ShopifyOrdersResponse>(ct);

                if (data?.Orders != null)
                    allOrders.AddRange(data.Orders);

                nextPageUrl = null;

                // 🔁 Shopify pagination (TEK DOĞRU YOL)
                if (response.Headers.TryGetValues("Link", out var links))
                {
                    var linkHeader = links.FirstOrDefault();
                    nextPageUrl = ExtractNextLink(linkHeader);
                }
            }

            return allOrders;
        }

        private static string? ExtractNextLink(string? linkHeader)
        {
            if (string.IsNullOrWhiteSpace(linkHeader))
                return null;

            var parts = linkHeader.Split(',');
            foreach (var part in parts)
            {
                if (part.Contains("rel=\"next\""))
                {
                    var start = part.IndexOf('<') + 1;
                    var end = part.IndexOf('>');
                    return part[start..end];
                }
            }
            return null;
        }
    }
}
