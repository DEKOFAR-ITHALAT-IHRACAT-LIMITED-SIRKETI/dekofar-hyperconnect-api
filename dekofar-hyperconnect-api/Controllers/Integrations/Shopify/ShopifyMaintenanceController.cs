using Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dekofar.HyperConnect.Api.Controllers.Integrations
{
    /// <summary>
    /// Shopify bakım / manuel müdahale endpoint’leri
    /// ⚠️ Webhook akışını etkilemez
    /// ⚠️ Sadece manuel reset & reprocess amaçlıdır
    /// </summary>
    [ApiController]
    [Route("api/integrations/shopify")]
    public sealed class ShopifyMaintenanceController : ControllerBase
    {
        private readonly ShopifyOrderReprocessService _reprocessService;

        public ShopifyMaintenanceController(
            ShopifyOrderReprocessService reprocessService)
        {
            _reprocessService = reprocessService;
        }

        /// <summary>
        /// ✅ AÇIK siparişleri kurallara göre yeniden etiketler
        ///
        /// AŞAMA 1:
        /// - Tüm açık siparişlerin TÜM etiketlerini kaldırır
        ///
        /// AŞAMA 2:
        /// - Yazdığımız business kurallarına göre
        ///   (ara1 / dhl / iptal)
        ///   yeniden etiketleme yapar
        ///
        /// ⚠️ Shopify API limitlerine uyumludur
        /// ⚠️ 100’lük batch + cursor pagination kullanır
        /// ⚠️ Webhook sistemini bozmaz
        /// </summary>
        [HttpPost("reprocess")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReprocessOpenOrders(
            [FromQuery] string shop,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(shop))
                return BadRequest("shop query param is required");

            var processed =
                await _reprocessService.ReprocessOpenOrdersAsync(
                    shop,
                    ct);

            return Ok(new
            {
                shop,
                processed,
                status = "completed"
            });
        }
    }
}
