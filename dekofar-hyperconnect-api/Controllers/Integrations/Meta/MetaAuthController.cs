using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Dekofar.HyperConnect.Integrations.Meta.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace dekofar_hyperconnect_api.Controllers.Integrations.Meta
{
    [ApiController]
    [Route("api/integrations/meta/auth")]
    public class MetaAuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public MetaAuthController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("login-url")]
        public IActionResult GetLoginUrl()
        {
            var appId = _configuration["Meta:AppId"];
            var redirectUri = _configuration["Meta:RedirectUri"];

            var scopes = string.Join(",",
                new[]
                {
                    "ads_read",
                    "pages_read_engagement",
                    "pages_manage_metadata",
                    "pages_manage_posts"
                    // instagram_basic, instagram_manage_comments ileride eklersin
                });

            var url =
                $"https://www.facebook.com/v20.0/dialog/oauth" +
                $"?client_id={appId}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri!)}" +
                $"&scope={Uri.EscapeDataString(scopes)}";

            return Ok(new { url });
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string? error)
        {
            if (!string.IsNullOrEmpty(error))
                return BadRequest(new { error });

            var appId = _configuration["Meta:AppId"];
            var appSecret = _configuration["Meta:AppSecret"];
            var redirectUri = _configuration["Meta:RedirectUri"];

            var client = _httpClientFactory.CreateClient();

            var tokenUrl =
                $"https://graph.facebook.com/v20.0/oauth/access_token" +
                $"?client_id={appId}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri!)}" +
                $"&client_secret={appSecret}" +
                $"&code={code}";

            var tokenResponse =
                await client.GetFromJsonAsync<FacebookAccessTokenResponse>(tokenUrl);

            if (tokenResponse == null)
                return StatusCode(500, "Failed to exchange code for access token.");

            // TODO: burada access_token'ı kendi kullanıcı tablonla ilişkilendirip DB'ye kaydet
            // Şimdilik direkt payload'ı dönelim
            return Ok(tokenResponse);
        }
    }
}
