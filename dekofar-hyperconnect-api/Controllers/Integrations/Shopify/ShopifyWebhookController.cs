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

        public ShopifyWebhookController(
            ShopifyOrderAutoTagService autoTagService)
        {
            _autoTagService = autoTagService;
        }

        /// <summary>
        /// Shopify → Order Created Webhook
        /// Bu endpoint otomatik etiketleme yapar
        /// </summary>
        /// <remarks>
        /// • Shopify tarafından çağrılır  
        /// • Swagger / manuel reprocess ile çakışmaz  
        /// • Manuel reset edilmiş siparişleri atlar  
        /// • Aynı telefon → sadece AÇIK siparişler ara1  
        /// </remarks>
        [HttpPost("orders/create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> OrderCreated(
            [FromBody] JObject payload,
            CancellationToken ct)
        {
            if (payload == null)
                return BadRequest("Payload is required");

            var shopDomain =
                Request.Headers["X-Shopify-Shop-Domain"]
                    .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(shopDomain))
                return BadRequest("Missing X-Shopify-Shop-Domain header");

            // =====================================================
            // 🔒 MANUEL RESET KORUMASI
            // =====================================================
            var note = payload["note"]?.ToString();

            if (!string.IsNullOrWhiteSpace(note) &&
                note.Contains(ShopifySystemNotes.ResetFlag))
            {
                // Manuel reset sonrası webhook → ETİKETLEME YAPMA
                return Ok();
            }

            // =====================================================
            // 🔥 OTOMATİK ETİKETLEME
            // =====================================================
            await _autoTagService.ApplyAutoTagsAsync(
                payload,
                shopDomain,
                ct,
                replaceExistingTags: true);

            // Shopify webhook'ları için 200 OK yeterlidir
            return Ok();
        }
    }
}
