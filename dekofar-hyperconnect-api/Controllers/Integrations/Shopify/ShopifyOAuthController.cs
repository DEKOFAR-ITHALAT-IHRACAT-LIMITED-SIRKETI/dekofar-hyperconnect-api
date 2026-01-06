using Dekofar.HyperConnect.Infrastructure.Persistence;
using Dekofar.HyperConnect.Integrations.Shopify.Common;
using Dekofar.HyperConnect.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace dekofar_hyperconnect_api.Controllers.Integrations.Shopify
{
    [ApiController]
    [Route("api/integrations/shopify/oauth")]
    public class ShopifyOAuthController : ControllerBase
    {
        private readonly ShopifyOptions _options;
        private readonly ApplicationDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;

        public ShopifyOAuthController(
            IOptions<ShopifyOptions> options,
            ApplicationDbContext db,
            IHttpClientFactory httpClientFactory)
        {
            _options = options.Value;
            _db = db;
            _httpClientFactory = httpClientFactory;
        }

        // =====================================================
        // 1️⃣ INSTALL / AUTHORIZE
        // =====================================================
        // GET:
        // /api/integrations/shopify/oauth/start?shop=xxx.myshopify.com
        // =====================================================
        [HttpGet("start")]
        public IActionResult Start([FromQuery] string shop)
        {
            if (string.IsNullOrWhiteSpace(shop))
                return BadRequest("shop query param is required");

            var redirectUri =
                $"{_options.AppUrl}/api/integrations/shopify/oauth/callback";

            var state = Guid.NewGuid().ToString("N");

            var authorizeUrl =
                $"https://{shop}/admin/oauth/authorize" +
                $"?client_id={_options.ClientId}" +
                $"&scope={_options.Scopes}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                $"&state={state}";

            return Redirect(authorizeUrl);
        }

        // =====================================================
        // 2️⃣ CALLBACK
        // =====================================================
        // Shopify buraya redirect eder
        // =====================================================
        [HttpGet("callback")]
        public async Task<IActionResult> Callback(
            [FromQuery] string shop,
            [FromQuery] string code,
            [FromQuery] string hmac,
            [FromQuery] string state)
        {
            if (string.IsNullOrWhiteSpace(shop) ||
                string.IsNullOrWhiteSpace(code) ||
                string.IsNullOrWhiteSpace(hmac))
            {
                return BadRequest("Missing required parameters");
            }

            // =====================================================
            // 🔐 HMAC DOĞRULAMA (QUERY STRING)
            // =====================================================
            var message = Request.Query
                .Where(x => x.Key != "hmac")
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}={x.Value}")
                .Aggregate((a, b) => $"{a}&{b}");

            var secretBytes = Encoding.UTF8.GetBytes(_options.ClientSecret);
            using var hmacSha256 = new HMACSHA256(secretBytes);
            var hashBytes = hmacSha256.ComputeHash(
                Encoding.UTF8.GetBytes(message));

            var calculatedHmac =
                Convert.ToHexString(hashBytes).ToLowerInvariant();

            if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(calculatedHmac),
                Encoding.UTF8.GetBytes(hmac)))
            {
                return Unauthorized("Invalid HMAC");
            }

            // =====================================================
            // 🔑 ACCESS TOKEN AL
            // =====================================================
            var client = _httpClientFactory.CreateClient();

            var tokenResponse = await client.PostAsJsonAsync(
                $"https://{shop}/admin/oauth/access_token",
                new
                {
                    client_id = _options.ClientId,
                    client_secret = _options.ClientSecret,
                    code = code
                });

            if (!tokenResponse.IsSuccessStatusCode)
            {
                var error = await tokenResponse.Content.ReadAsStringAsync();
                return StatusCode(500, error);
            }

            var tokenJson = await tokenResponse.Content
                .ReadFromJsonAsync<JsonElement>();

            var accessToken =
                tokenJson.GetProperty("access_token").GetString()!;
            var scopes =
                tokenJson.GetProperty("scope").GetString()!;

            // =====================================================
            // 💾 DB KAYDI (ShopifyStore)
            // =====================================================
            var store = await _db.Set<ShopifyStore>()
                .FirstOrDefaultAsync(x => x.ShopDomain == shop);

            if (store == null)
            {
                store = new ShopifyStore
                {
                    Id = Guid.NewGuid(),
                    ShopDomain = shop,
                    AccessToken = accessToken,
                    Scopes = scopes,
                    InstalledAtUtc = DateTime.UtcNow
                };

                _db.Add(store);
            }
            else
            {
                store.AccessToken = accessToken;
                store.Scopes = scopes;
                store.InstalledAtUtc = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            // =====================================================
            // ✅ SUCCESS
            // =====================================================
            return Ok(new
            {
                success = true,
                shop = shop,
                scopes = scopes,
                tokenPrefix = accessToken.Substring(0, 5) // shpat_
            });
        }

        // =====================================================
        // 🧪 TOKEN TEST (Swagger için)
        // =====================================================
        [HttpGet("test-token")]
        public async Task<IActionResult> TestToken([FromQuery] string shop)
        {
            if (string.IsNullOrWhiteSpace(shop))
                return BadRequest("shop is required");

            var store = await _db.Set<ShopifyStore>()
                .FirstOrDefaultAsync(x => x.ShopDomain == shop);

            if (store == null)
                return NotFound("Shop not installed");

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri($"https://{shop}");
            client.DefaultRequestHeaders.Add(
                "X-Shopify-Access-Token", store.AccessToken);

            var gqlQuery = new
            {
                query = @"query {
            shop {
                name
                myshopifyDomain
            }
        }"
            };

            var response = await client.PostAsJsonAsync(
                "/admin/api/2024-04/graphql.json",
                gqlQuery);

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode(500, body);

            return Ok(new
            {
                success = true,
                shop,
                response = body
            });
        }

    }


}
