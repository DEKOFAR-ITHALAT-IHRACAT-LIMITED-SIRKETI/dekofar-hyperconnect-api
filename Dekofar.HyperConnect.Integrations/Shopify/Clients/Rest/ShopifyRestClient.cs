using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models.Raw;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Dekofar.HyperConnect.Integrations.Shopify.Clients.Rest
{
    /// <summary>
    /// Shopify REST Client (SIMPLE MODE)
    /// ✔ OAuth token ile direkt erişim
    /// ✔ Pagination (Link header)
    /// ✔ Stateless
    /// </summary>
    public sealed class ShopifyRestClient
    {
        private readonly ILogger<ShopifyRestClient> _logger;
        private const string ApiVersion = "2026-01";

        public ShopifyRestClient(
            ILogger<ShopifyRestClient> logger)
        {
            _logger = logger;
        }

        public async Task<List<Order>> GetAllOrdersAsync(
            string shopDomain,
            string accessToken,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(shopDomain))
                throw new ArgumentException("shopDomain is required");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("accessToken is required");

            using var client = new HttpClient
            {
                BaseAddress = new Uri($"https://{shopDomain}")
            };

            client.DefaultRequestHeaders.Add(
                "X-Shopify-Access-Token",
                accessToken);

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var allOrders = new List<Order>();

            string? nextPageUrl =
                $"/admin/api/{ApiVersion}/orders.json?limit=250&status=any";

            while (!string.IsNullOrWhiteSpace(nextPageUrl))
            {
                var response = await client.GetAsync(nextPageUrl, ct);
                var body = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "SHOPIFY REST ERROR → {Status} | {Body}",
                        response.StatusCode,
                        body);

                    throw new HttpRequestException(
                        $"Shopify REST HTTP {(int)response.StatusCode}");
                }

                var data =
                    await response.Content
                        .ReadFromJsonAsync<ShopifyOrdersResponse>(
                            cancellationToken: ct);

                if (data?.Orders is { Count: > 0 })
                    allOrders.AddRange(data.Orders);

                nextPageUrl = null;

                if (response.Headers.TryGetValues("Link", out var links))
                {
                    nextPageUrl = ExtractNextLink(links.FirstOrDefault());
                }
            }

            return allOrders;
        }

        // =====================================================
        // 🔁 LINK HEADER PARSER
        // =====================================================
        private static string? ExtractNextLink(string? linkHeader)
        {
            if (string.IsNullOrWhiteSpace(linkHeader))
                return null;

            foreach (var part in linkHeader.Split(','))
            {
                if (!part.Contains("rel=\"next\"", StringComparison.OrdinalIgnoreCase))
                    continue;

                var start = part.IndexOf('<') + 1;
                var end = part.IndexOf('>');

                if (start > 0 && end > start)
                    return part[start..end];
            }

            return null;
        }
    }
}
