using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl
{
    public sealed class ShopifyGraphQlClient
    {
        private readonly ILogger<ShopifyGraphQlClient> _logger;
        private const string ApiVersion = "2026-01";

        public ShopifyGraphQlClient(
            ILogger<ShopifyGraphQlClient> logger)
        {
            _logger = logger;
        }

        public async Task<JObject> ExecuteAsync(
            string shopDomain,
            string accessToken,
            string query,
            object? variables = null,
            CancellationToken ct = default)
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

            var payload = JsonConvert.SerializeObject(new
            {
                query,
                variables
            });

            using var content = new StringContent(
                payload,
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                $"/admin/api/{ApiVersion}/graphql.json",
                content,
                ct);

            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "SHOPIFY HTTP ERROR → {Status} | {Body}",
                    response.StatusCode,
                    body);

                throw new HttpRequestException(
                    $"Shopify HTTP {(int)response.StatusCode}");
            }

            var json = JObject.Parse(body);

            if (json["errors"] != null)
            {
                _logger.LogError(
                    "SHOPIFY GRAPHQL ERROR → {Errors}",
                    json["errors"]!.ToString());

                throw new InvalidOperationException(
                    "Shopify GraphQL error");
            }

            return json;
        }
    }
}
