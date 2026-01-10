using Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Dekofar.HyperConnect.Api.Controllers.Integrations
{
    [ApiController]
    [Route("api/integrations/shopify/webhooks")]
    public class ShopifyWebhooksController : ControllerBase
    {
        private readonly ShopifyOrderAutoTagService _autoTagService;
        private readonly ILogger<ShopifyWebhooksController> _logger;
        private readonly string _webhookSecret;

        public ShopifyWebhooksController(
            ShopifyOrderAutoTagService autoTagService,
            IConfiguration configuration,
            ILogger<ShopifyWebhooksController> logger)
        {
            _autoTagService = autoTagService;
            _logger = logger;
            _webhookSecret =
                configuration["Shopify:WebhookSecret"]
                ?? throw new InvalidOperationException("Shopify WebhookSecret missing");
        }

        // =====================================================
        // 🛒 ORDER CREATE
        // =====================================================
        [HttpPost("orders/create")]
        public async Task<IActionResult> OrderCreated(
            [FromBody] JObject payload,
            CancellationToken ct)
        {
            if (!VerifyHmac(Request, payload.ToString()))
                return Unauthorized();

            var shopDomain =
                Request.Headers["X-Shopify-Shop-Domain"].ToString();

            if (string.IsNullOrWhiteSpace(shopDomain))
                return BadRequest("Missing shop domain");

            await _autoTagService.ApplyAutoTagsAsync(
                payload,
                shopDomain,
                ct,
                replaceExistingTags: true);

            return Ok();
        }

        // =====================================================
        // 🔄 ORDER UPDATE
        // =====================================================
        [HttpPost("orders/updated")]
        public async Task<IActionResult> OrderUpdated(
            [FromBody] JObject payload,
            CancellationToken ct)
        {
            if (!VerifyHmac(Request, payload.ToString()))
                return Unauthorized();

            var shopDomain =
                Request.Headers["X-Shopify-Shop-Domain"].ToString();

            if (string.IsNullOrWhiteSpace(shopDomain))
                return BadRequest("Missing shop domain");

            await _autoTagService.ApplyAutoTagsAsync(
                payload,
                shopDomain,
                ct,
                replaceExistingTags: false);

            return Ok();
        }

        // =====================================================
        // 🔐 HMAC VERIFY
        // =====================================================
        private bool VerifyHmac(
            HttpRequest request,
            string body)
        {
            if (!request.Headers.TryGetValue(
                    "X-Shopify-Hmac-Sha256",
                    out var hmacHeader))
                return false;

            using var hmac =
                new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));

            var hash =
                hmac.ComputeHash(Encoding.UTF8.GetBytes(body));

            var calculated =
                Convert.ToBase64String(hash);

            return calculated == hmacHeader;
        }
    }
}
