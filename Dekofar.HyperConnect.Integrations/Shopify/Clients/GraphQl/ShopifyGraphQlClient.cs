using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl
{
    public class ShopifyGraphQlClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<ShopifyGraphQlClient> _logger;

        public ShopifyGraphQlClient(
            HttpClient http,
            ILogger<ShopifyGraphQlClient> logger)
        {
            _http = http;
            _logger = logger;

            // 🔒 Default headers (tek sefer)
            if (!_http.DefaultRequestHeaders.Accept.Any())
            {
                _http.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }

        /// <summary>
        /// Shopify GraphQL execute helper
        /// </summary>
        public async Task<JObject> ExecuteAsync(
            string query,
            object? variables = null,
            CancellationToken ct = default)
        {
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

            try
            {
                response = await _http.PostAsync(
                    "/admin/api/2024-04/graphql.json",
                    content,
                    ct);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "SHOPIFY GRAPHQL REQUEST CANCELLED");
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

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "SHOPIFY GRAPHQL HTTP ERROR → Status={Status}, Body={Body}",
                    response.StatusCode,
                    responseBody);

                response.EnsureSuccessStatusCode();
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

            // 🔴 GraphQL-level errors
            if (obj["errors"] != null)
            {
                _logger.LogError(
                    "SHOPIFY GRAPHQL ERROR → Query={Query}, Errors={Errors}",
                    query,
                    obj["errors"]!.ToString(Formatting.None));

                throw new InvalidOperationException(
                    $"Shopify GraphQL Error: {obj["errors"]}");
            }

            return obj;
        }
    }
}
