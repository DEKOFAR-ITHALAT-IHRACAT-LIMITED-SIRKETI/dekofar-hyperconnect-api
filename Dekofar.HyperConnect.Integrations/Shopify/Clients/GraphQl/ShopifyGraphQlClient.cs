using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl
{
    public sealed class ShopifyGraphQlClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ShopifyGraphQlClient> _logger;

        private const string ApiVersion = "2026-01";

        public ShopifyGraphQlClient(
            HttpClient httpClient,
            ILogger<ShopifyGraphQlClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<JObject> ExecuteAsync(
            string shopDomain,
            string accessToken,
            string query,
            object? variables = null,
            CancellationToken ct = default)
        {
            _httpClient.BaseAddress = new Uri($"https://{shopDomain}");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(
                "X-Shopify-Access-Token",
                accessToken);

            _httpClient.DefaultRequestHeaders.Accept.Add(
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

            var response = await _httpClient.PostAsync(
                $"/admin/api/{ApiVersion}/graphql.json",
                content,
                ct);

            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "SHOPIFY GRAPHQL HTTP ERROR → {Status} | {Body}",
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
