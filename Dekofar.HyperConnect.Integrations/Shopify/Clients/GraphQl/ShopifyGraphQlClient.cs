using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl
{
    /// <summary>
    /// Shopify GraphQL Client
    /// ✔ ENV based token
    /// ✔ Shopify 2026-01 uyumlu
    /// ✔ Güçlü error handling
    /// </summary>
    public class ShopifyGraphQlClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<ShopifyGraphQlClient> _logger;

        private const string DefaultApiVersion = "2026-01";

        public ShopifyGraphQlClient(
            HttpClient http,
            ILogger<ShopifyGraphQlClient> logger)
        {
            _http = http;
            _logger = logger;

            if (!_http.DefaultRequestHeaders.Accept.Any())
            {
                _http.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }

        // =====================================================
        // 🚀 EXECUTE
        // =====================================================
        public async Task<JObject> ExecuteAsync(
            string query,
            object? variables = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("GraphQL query is empty");

            var payload = new
            {
                query,
                variables
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);

            using var content = new StringContent(
                jsonPayload,
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response;

            var endpoint = $"/admin/api/{DefaultApiVersion}/graphql.json";

            try
            {
                response = await _http.PostAsync(
                    endpoint,
                    content,
                    ct);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("SHOPIFY GRAPHQL REQUEST CANCELLED");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "SHOPIFY GRAPHQL HTTP REQUEST FAILED");
                throw;
            }

            var responseBody = await response.Content
                .ReadAsStringAsync(ct);

            // =====================================================
            // ❌ HTTP LEVEL ERRORS
            // =====================================================
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogCritical(
                        "SHOPIFY UNAUTHORIZED ❌ Access token invalid or missing");
                }
                else if (response.StatusCode == (HttpStatusCode)429)
                {
                    _logger.LogWarning(
                        "SHOPIFY RATE LIMIT ⚠️ Too many requests");
                }

                _logger.LogError(
                    "SHOPIFY GRAPHQL HTTP ERROR → Status={Status}, Body={Body}",
                    response.StatusCode,
                    responseBody);

                throw new HttpRequestException(
                    $"Shopify GraphQL HTTP {(int)response.StatusCode}");
            }

            JObject obj;

            try
            {
                obj = JObject.Parse(responseBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "SHOPIFY GRAPHQL INVALID JSON RESPONSE → {Body}",
                    responseBody);

                throw new InvalidOperationException(
                    "Invalid JSON response from Shopify GraphQL");
            }

            // =====================================================
            // 🔴 GRAPHQL ERRORS
            // =====================================================
            if (obj["errors"] != null)
            {
                _logger.LogError(
                    "SHOPIFY GRAPHQL ERROR → {Errors}",
                    obj["errors"]!.ToString(Formatting.None));

                throw new InvalidOperationException(
                    $"Shopify GraphQL Error: {obj["errors"]}");
            }

            // =====================================================
            // 🔴 USER ERRORS (mutation)
            // =====================================================
            var userErrors = obj.SelectTokens("$..userErrors[*]")
                .Select(e => e.ToString(Formatting.None))
                .ToArray();

            if (userErrors.Length > 0)
            {
                _logger.LogError(
                    "SHOPIFY GRAPHQL USER ERRORS → {Errors}",
                    string.Join(" | ", userErrors));

                throw new InvalidOperationException(
                    $"Shopify GraphQL UserErrors: {string.Join(" | ", userErrors)}");
            }

            return obj;
        }
    }
}
