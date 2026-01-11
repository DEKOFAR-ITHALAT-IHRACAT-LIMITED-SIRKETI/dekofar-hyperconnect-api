using Dekofar.HyperConnect.Integrations.Shopify.Constants;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Api.Controllers.Integrations.Shopify
{
    [ApiController]
    [Route("api/integrations/shopify/webhooks")]
    public sealed class ShopifyWebhookController : ControllerBase
    {
        private readonly ShopifyOrderAutoTagService _autoTagService;

        public ShopifyWebhookController(ShopifyOrderAutoTagService autoTagService)
        {
            _autoTagService = autoTagService;
        }

        [HttpPost("orders/create")]
        public async Task<IActionResult> OrderCreated(
            [FromBody] JObject payload,
            CancellationToken ct)
        {
            var shop =
                Request.Headers["X-Shopify-Shop-Domain"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(shop))
                return BadRequest();

            var note = payload["note"]?.ToString();
            if (!string.IsNullOrWhiteSpace(note) &&
                note.Contains(ShopifySystemNotes.ResetFlag))
                return Ok();

            await _autoTagService.ApplyAutoTagsAsync(
                payload,
                shop,
                ct,
                replaceExistingTags: true);

            return Ok();
        }
    }
}
