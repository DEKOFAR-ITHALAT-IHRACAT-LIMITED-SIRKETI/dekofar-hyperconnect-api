using Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Api.Controllers.Integrations.Shopify
{
    [ApiController]
    [Route("api/integrations/shopify/webhooks")]
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
            var shopDomain =
                Request.Headers["X-Shopify-Shop-Domain"]
                    .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(shopDomain))
                return BadRequest("Missing shop domain");

            await _autoTagService.ApplyAutoTagsAsync(
                payload,
                shopDomain,
                ct);

            return Ok();
        }
    }
}
