using Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Api.Controllers.Webhooks
{
    [ApiController]
    [Route("api/webhooks/shopify")]
    public class ShopifyWebhookController : ControllerBase
    {
        private readonly ShopifyOrderAutoTagService _autoTagService;

        public ShopifyWebhookController(
            ShopifyOrderAutoTagService autoTagService)
        {
            _autoTagService = autoTagService;
        }

        /// <summary>
        /// Shopify → Order Created Webhook
        /// </summary>
        [HttpPost("orders/create")]
        public async Task<IActionResult> OrderCreated(
            [FromBody] JObject payload,
            CancellationToken ct)
        {
            // Shopify bu header'ı ZORUNLU gönderir
            var shopDomain =
                Request.Headers["X-Shopify-Shop-Domain"]
                    .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(shopDomain))
                return BadRequest("X-Shopify-Shop-Domain header missing");

            await _autoTagService.ApplyAutoTagsAsync(
                order: payload,
                shopDomain: shopDomain,
                ct: ct,
                replaceExistingTags: true
            );

            return Ok();
        }
    }
}
