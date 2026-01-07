using Dekofar.HyperConnect.Domain.Entities;
using Dekofar.HyperConnect.Infrastructure.Persistence;
using Dekofar.HyperConnect.Integrations.Shopify.Common;
using Dekofar.HyperConnect.Integrations.Shopify.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace dekofar_hyperconnect_api.Controllers.Integrations.Shopify
{
    /// <summary>
    /// Shopify OAuth Controller
    /// ✔ Shopify resmi OAuth akışı
    /// ✔ HMAC secure
    /// ✔ Multi-store uyumlu
    /// </summary>
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
        [HttpGet("start")]
        public IActionResult Start([FromQuery] string shop)
        {
            if (string.IsNullOrWhiteSpace(shop))
                return BadRequest("shop query param is required");

            if (!shop.EndsWith(".myshopify.com", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid shop domain");

            var redirectUri =
                $"{_options.AppUrl}/api/integrations/shopify/oauth/callback";

            var state = Guid.NewGuid().ToString("N");

            Response.Cookies.Append(
                "shopify_oauth_state",
                state,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    MaxAge = TimeSpan.FromMinutes(5)
                });

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
        [HttpGet("callback")]
        public async Task<IActionResult> Callback(
            [FromQuery] string shop,
            [FromQuery] string code,
            [FromQuery] string hmac,
            [FromQuery] string state)
        {
            if (string.IsNullOrWhiteSpace(shop) ||
                string.IsNullOrWhiteSpace(code) ||
                string.IsNullOrWhiteSpace(hmac) ||
                string.IsNullOrWhiteSpace(state))
            {
                return BadRequest("Missing required parameters");
            }

            if (!Request.Cookies.TryGetValue(
                    "shopify_oauth_state",
                    out var storedState) ||
                storedState != state)
            {
                return Unauthorized("Invalid state");
            }

            var query = Request.Query
                .ToDictionary(x => x.Key, x => x.Value.ToString());

            if (!ShopifyHmacValidator.ValidateOAuth(
                    query,
                    _options.ClientSecret))
            {
                return Unauthorized("Invalid HMAC");
            }

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

            var store = await _db.ShopifyStores
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

                _db.ShopifyStores.Add(store);
            }
            else
            {
                store.AccessToken = accessToken;
                store.Scopes = scopes;
                store.InstalledAtUtc = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                shop,
                scopes,
                tokenPrefix = accessToken[..5]
            });
        }

        // =====================================================
        // 🧪 TOKEN TEST
        // =====================================================
        [HttpGet("test-token")]
        public async Task<IActionResult> TestToken([FromQuery] string shop)
        {
            var store = await _db.ShopifyStores
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ShopDomain == shop);

            if (store == null)
                return NotFound("Shop not installed");

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri($"https://{shop}");
            client.DefaultRequestHeaders.Add(
                "X-Shopify-Access-Token",
                store.AccessToken);

            var response = await client.PostAsJsonAsync(
                "/admin/api/2024-04/graphql.json",
                new
                {
                    query = @"query {
                      shop {
                        name
                        myshopifyDomain
                      }
                    }"
                });

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode(500, body);

            return Ok(new
            {
                success = true,
                response = body
            });
        }
    }
}
