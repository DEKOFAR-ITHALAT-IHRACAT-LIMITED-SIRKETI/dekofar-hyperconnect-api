using Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;
using Dekofar.HyperConnect.Integrations.Shopify.Utils;
using Dekofar.HyperConnect.Infrastructure.Persistence;
using Dekofar.HyperConnect.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text;

namespace dekofar_hyperconnect_api.Controllers.Integrations.Shopify
{
    [ApiController]
    [Route("api/integrations/shopify")]
    public class ShopifyOrderWebhookController : ControllerBase
    {
        private readonly ShopifyOrderAutoTagService _autoTagService;
        private readonly ApplicationDbContext _db;
        private readonly string _webhookSecret;

        public ShopifyOrderWebhookController(
            ShopifyOrderAutoTagService autoTagService,
            ApplicationDbContext db)
        {
            _autoTagService = autoTagService;
            _db = db;

            _webhookSecret =
                Environment.GetEnvironmentVariable("SHOPIFY_WEBHOOK_SECRET")
                ?? throw new InvalidOperationException(
                    "SHOPIFY_WEBHOOK_SECRET env var missing");
        }

        [HttpPost("orders/create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> OrderCreated(CancellationToken ct)
        {
            // =====================================================
            // 1️⃣ HMAC HEADER
            // =====================================================
            if (!Request.Headers.TryGetValue(
                    "X-Shopify-Hmac-Sha256",
                    out var hmacHeader))
            {
                return Unauthorized();
            }

            // =====================================================
            // 2️⃣ RAW BODY
            // =====================================================
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

            // =====================================================
            // 3️⃣ HMAC VALIDATION
            // =====================================================
            var isValid = ShopifyHmacValidator.Validate(
                body,
                hmacHeader!,
                _webhookSecret);

            if (!isValid)
                return Unauthorized();

            // =====================================================
            // 4️⃣ JSON PARSE
            // =====================================================
            JObject payload;
            try
            {
                payload = JObject.Parse(body);
            }
            catch
            {
                return Ok();
            }

            var shopifyOrderId =
                payload["id"]?.ToString();

            // =====================================================
            // 5️⃣ IDEMPOTENCY (aynı webhook tekrar gelirse)
            // =====================================================
            if (!string.IsNullOrWhiteSpace(shopifyOrderId))
            {
                var exists = _db.ShopifyWebhookEvents
                    .Any(x =>
                        x.Topic == "orders/create" &&
                        x.ExternalId == shopifyOrderId);

                if (exists)
                    return Ok();
            }

            // =====================================================
            // 6️⃣ DB KAYDI (AUDIT)
            // =====================================================
            _db.ShopifyWebhookEvents.Add(new ShopifyWebhookEvent
            {
                Topic = "orders/create",
                ExternalId = shopifyOrderId,
                Payload = body,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(ct);

            // =====================================================
            // 7️⃣ AUTO TAG
            // =====================================================
            await _autoTagService.ApplyAutoTagsAsync(
                payload,
                ct,
                replaceExistingTags: true);

            return Ok();
        }
    }
}
