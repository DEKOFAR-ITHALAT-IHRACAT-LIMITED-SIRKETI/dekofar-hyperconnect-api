using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl
{
    public sealed class ShopifyGraphQlClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ShopifyGraphQlClient> _logger;

        // Shopify stable version
        private const string ApiVersion = "2026-01";

        public ShopifyGraphQlClient(
            HttpClient httpClient,
            ILogger<ShopifyGraphQlClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // =====================================================
        // 🚀 EXECUTE GRAPHQL
        // =====================================================
        public async Task<JObject> ExecuteAsync(
            string shopDomain,
            string accessToken,
            string query,
            object? variables = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(shopDomain))
                throw new ArgumentException("shopDomain is required", nameof(shopDomain));

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("accessToken is required", nameof(accessToken));

            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("query is required", nameof(query));

            var url =
                $"https://{shopDomain}/admin/api/{ApiVersion}/graphql.json";

            var payload = JsonConvert.SerializeObject(new
            {
                query,
                variables
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Headers.Add("X-Shopify-Access-Token", accessToken);
            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            request.Content = new StringContent(
                payload,
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response;

            try
            {
                response = await _httpClient.SendAsync(request, ct);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "SHOPIFY GRAPHQL TIMEOUT → Shop={Shop}",
                    shopDomain);

                throw;
            }

            var body = await response.Content.ReadAsStringAsync(ct);

            // =====================================================
            // ❌ HTTP ERROR
            // =====================================================
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "SHOPIFY GRAPHQL HTTP ERROR → Shop={Shop} Status={Status}\n{Body}",
                    shopDomain,
                    response.StatusCode,
                    body);

                throw new HttpRequestException(
                    $"Shopify HTTP {(int)response.StatusCode}");
            }

            // =====================================================
            // 🧪 JSON PARSE (SAFE)
            // =====================================================
            JObject json;
            try
            {
                json = JObject.Parse(body);
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "SHOPIFY GRAPHQL INVALID JSON → Shop={Shop}\n{Body}",
                    shopDomain,
                    body);

                throw;
            }

            // =====================================================
            // ❌ TOP-LEVEL GRAPHQL ERRORS
            // =====================================================
            if (json["errors"] is JArray errors && errors.Count > 0)
            {
                _logger.LogError(
                    "SHOPIFY GRAPHQL ERRORS → Shop={Shop}\n{Errors}",
                    shopDomain,
                    errors.ToString());

                throw new InvalidOperationException(
                    "Shopify GraphQL returned errors");
            }

            // =====================================================
            // ⚠️ MUTATION USER ERRORS (non-fatal)
            // =====================================================
            var userErrors = json
                .SelectTokens("$..userErrors[*].message")
                .Select(t => t.ToString())
                .ToList();

            if (userErrors.Count > 0)
            {
                _logger.LogWarning(
                    "SHOPIFY GRAPHQL USER ERRORS → Shop={Shop}\n{Errors}",
                    shopDomain,
                    string.Join(" | ", userErrors));
            }

            // =====================================================
            // ⚠️ DATA YOKSA (defansif log)
            // =====================================================
            if (json["data"] == null)
            {
                _logger.LogWarning(
                    "SHOPIFY GRAPHQL RESPONSE WITHOUT DATA → Shop={Shop}\n{Body}",
                    shopDomain,
                    body);
            }

            return json;
        }
    }
}
