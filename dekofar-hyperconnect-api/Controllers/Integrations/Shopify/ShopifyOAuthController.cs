using Dekofar.HyperConnect.Application.Integrations.Shopify.Services;
using Dekofar.HyperConnect.Integrations.Shopify.Common;
using Dekofar.HyperConnect.Integrations.Shopify.OAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace dekofar_hyperconnect_api.Controllers.Integrations.Shopify
{
    [ApiController]
    [Route("api/integrations/shopify/oauth")]
    public class ShopifyOAuthController : ControllerBase
    {
        private readonly ShopifyOptions _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ShopifyStoreService _storeService;

        public ShopifyOAuthController(
            IOptions<ShopifyOptions> options,
            IHttpClientFactory httpClientFactory,
            ShopifyStoreService storeService)
        {
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
            _storeService = storeService;
        }

        // =====================================================
        // 1️⃣ INSTALL
        // =====================================================
        [HttpGet("install")]
        public IActionResult Install([FromQuery] string shop)
        {
            if (string.IsNullOrWhiteSpace(shop))
                return BadRequest("Shop is required");

            var redirectUri =
                $"{_options.AppUrl}/api/integrations/shopify/oauth/callback";

            var url =
                $"https://{shop}/admin/oauth/authorize" +
                $"?client_id={_options.ClientId}" +
                $"&scope={_options.Scopes}" +
                $"&redirect_uri={redirectUri}";

            return Redirect(url);
        }

        // =====================================================
        // 2️⃣ CALLBACK
        // =====================================================
        [HttpGet("callback")]
        public async Task<IActionResult> Callback(
            [FromQuery] string shop,
            [FromQuery] string code)
        {
            if (string.IsNullOrWhiteSpace(shop) || string.IsNullOrWhiteSpace(code))
                return BadRequest("Missing shop or code");

            // 🔐 HMAC doğrulaması
            if (!IsValidShopifyHmac(_options.ClientSecret))
                return Unauthorized("Invalid HMAC");

            var client = _httpClientFactory.CreateClient();

            var tokenResponse = await client.PostAsJsonAsync(
                $"https://{shop}/admin/oauth/access_token",
                new
                {
                    client_id = _options.ClientId,
                    client_secret = _options.ClientSecret,
                    code
                });

            if (!tokenResponse.IsSuccessStatusCode)
                return BadRequest("Token exchange failed");

            var payload =
                await tokenResponse.Content.ReadFromJsonAsync<ShopifyTokenResponse>();

            if (payload == null || string.IsNullOrWhiteSpace(payload.access_token))
                return BadRequest("Invalid token response");

            // ✅ DB’ye kaydet
            await _storeService.UpsertAsync(
                shopDomain: shop,
                accessToken: payload.access_token,
                scopes: _options.Scopes
            );

            return Ok(new
            {
                success = true,
                shop
            });
        }

        // =====================================================
        // 🔐 PRIVATE HMAC VALIDATOR (KESİN ÇALIŞIR)
        // =====================================================
        private bool IsValidShopifyHmac(string clientSecret)
        {
            if (!Request.Query.TryGetValue("hmac", out var hmacValues))
                return false;

            var receivedHmac = hmacValues.ToString();

            var sortedQuery = Request.Query
                .Where(x => x.Key != "hmac" && x.Key != "signature")
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}={x.Value.ToString()}")
                .ToArray();

            var message = string.Join("&", sortedQuery);

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(clientSecret));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));

            var calculatedHmac =
                Convert.ToHexString(hashBytes).ToLowerInvariant();

            return calculatedHmac == receivedHmac;
        }

    }
}
