using Dekofar.HyperConnect.Integrations.Shopify.Common;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;
using Dekofar.HyperConnect.Integrations.Shopify.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;

namespace dekofar_hyperconnect_api.Controllers.Integrations.Shopify
{
    [ApiController]
    [Route("api/integrations/shopify")]
    public class ShopifyOrderWebhookController : ControllerBase
    {
        private readonly ShopifyOrderAutoTagService _autoTagService;
        private readonly ShopifyOptions _shopifyOptions;

        public ShopifyOrderWebhookController(
            ShopifyOrderAutoTagService autoTagService,
            IOptions<ShopifyOptions> shopifyOptions)
        {
            _autoTagService = autoTagService;
            _shopifyOptions = shopifyOptions.Value;
        }

        /// <summary>
        /// Shopify order/create webhook
        /// </summary>
        [HttpPost("orders/create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> OrderCreated(CancellationToken ct)
        {
            // 1️⃣ HMAC header kontrol
            if (!Request.Headers.TryGetValue(
                    "X-Shopify-Hmac-Sha256",
                    out var hmacHeader))
            {
                return Unauthorized();
            }

            // 2️⃣ RAW body oku
            Request.EnableBuffering();

            string body;
            using (var reader = new StreamReader(
                Request.Body,
                Encoding.UTF8,
                leaveOpen: true))
            {
                body = await reader.ReadToEndAsync(ct);
                Request.Body.Position = 0;
            }

            if (string.IsNullOrWhiteSpace(body))
                return Ok();

            // 3️⃣ HMAC doğrula
            var isValid = ShopifyHmacValidator.Validate(
                body,
                hmacHeader!,
                _shopifyOptions.WebhookSecret);

            if (!isValid)
            {
                return Unauthorized();
            }

            // 4️⃣ JSON parse
            var payload = JObject.Parse(body);

            // 5️⃣ Otomatik etiketleme
            await _autoTagService.ApplyAutoTagsAsync(payload, ct);

            return Ok();
        }
    }
}
