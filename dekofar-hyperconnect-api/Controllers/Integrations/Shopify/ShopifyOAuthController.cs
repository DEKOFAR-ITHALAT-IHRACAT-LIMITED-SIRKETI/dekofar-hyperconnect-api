using Dekofar.HyperConnect.Integrations.Shopify.Common;
using Dekofar.HyperConnect.Integrations.Shopify.OAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace dekofar_hyperconnect_api.Controllers.Integrations.Shopify
{
    [ApiController]
    [Route("api/integrations/shopify/oauth")]
    public class ShopifyOAuthController : ControllerBase
    {
        private readonly ShopifyOptions _options;
        private readonly IHttpClientFactory _http;

        public ShopifyOAuthController(
            IOptions<ShopifyOptions> options,
            IHttpClientFactory http)
        {
            _options = options.Value;
            _http = http;
        }

        // 1️⃣ INSTALL
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

        [HttpGet("callback")]
        public async Task<IActionResult> Callback(
            [FromQuery] string shop,
            [FromQuery] string code)
        {
            // 🔐 HMAC doğrulaması
            var query = Request.Query
                .ToDictionary(x => x.Key, x => x.Value.ToString());

            if (!ShopifyHmacValidator.IsValid(query, _options.ClientSecret))
                return Unauthorized("Invalid HMAC");

            var client = _http.CreateClient();

            var response = await client.PostAsJsonAsync(
                $"https://{shop}/admin/oauth/access_token",
                new
                {
                    client_id = _options.ClientId,
                    client_secret = _options.ClientSecret,
                    code
                });

            if (!response.IsSuccessStatusCode)
                return BadRequest("Token exchange failed");

            var payload =
                await response.Content.ReadFromJsonAsync<ShopifyTokenResponse>();

            return Ok(new
            {
                shop,
                accessToken = payload!.access_token
            });
        }

    }
}
