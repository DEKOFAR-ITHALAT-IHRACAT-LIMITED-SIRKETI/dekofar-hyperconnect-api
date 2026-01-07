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

        // 1️⃣ Install
        [HttpGet("install")]
        public IActionResult Install([FromQuery] string shop)
        {
            var redirectUri =
                $"{_options.AppUrl}/api/integrations/shopify/oauth/callback";

            var url =
                $"https://{shop}/admin/oauth/authorize" +
                $"?client_id={_options.ClientId}" +
                $"&scope={_options.Scopes}" +
                $"&redirect_uri={redirectUri}";

            return Redirect(url);
        }

        // 2️⃣ Callback
        [HttpGet("callback")]
        public async Task<IActionResult> Callback(
            [FromQuery] string shop,
            [FromQuery] string code)
        {
            var client = _http.CreateClient();

            var response = await client.PostAsJsonAsync(
                $"https://{shop}/admin/oauth/access_token",
                new
                {
                    client_id = _options.ClientId,
                    client_secret = _options.ClientSecret,
                    code
                });

            response.EnsureSuccessStatusCode();

            var payload =
                await response.Content.ReadFromJsonAsync<ShopifyTokenResponse>();

            // TODO: DB’ye kaydet
            // shop + payload.access_token

            return Ok(new
            {
                shop,
                token = payload!.access_token
            });
        }
    }
}
