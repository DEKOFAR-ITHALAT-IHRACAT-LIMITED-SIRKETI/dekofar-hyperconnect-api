using System.Threading;
using System.Threading.Tasks;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dekofar.HyperConnect.Api.Controllers.Integrations
{
    [ApiController]
    [Route("api/integrations/shopify")]
    public sealed class ShopifyMaintenanceController : ControllerBase
    {
        private readonly ShopifyOrderResetService _resetService;
        private readonly ShopifyOrderReprocessService _reprocessService;

        public ShopifyMaintenanceController(
            ShopifyOrderResetService resetService,
            ShopifyOrderReprocessService reprocessService)
        {
            _resetService = resetService;
            _reprocessService = reprocessService;
        }

        // =====================================================
        // 🔥 1️⃣ SADECE TAG RESET (Swagger)
        // =====================================================
        /// <summary>
        /// 🔥 Açık siparişlerdeki TÜM etiketleri temizler
        /// ❌ KURAL çalıştırmaz
        /// ❌ Etiket eklemez
        /// ✔ Sadece reset
        /// </summary>
        /// <remarks>
        /// Shopify API limitlerine uygun:
        /// 100’erli batch + cursor pagination
        /// </remarks>
        [HttpPost("reset-tags")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetOpenOrderTags(
            [FromQuery] string shop,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(shop))
                return BadRequest("shop query param is required");

            var cleared =
                await _resetService.ResetAllOpenOrderTagsAsync(
                    shop,
                    ct);

            return Ok(new
            {
                shop,
                cleared,
                status = "reset-completed"
            });
        }

        // =====================================================
        // 🔁 2️⃣ RESET SONRASI YENİDEN ETİKETLE
        // =====================================================
        /// <summary>
        /// 🔁 Açık siparişleri kurallara göre yeniden etiketler
        /// ✔ OrderDecisionEngine çalışır
        /// ✔ ara1 / dhl / iptal atanır
        /// </summary>
        /// <remarks>
        /// Reset işleminden SONRA çağrılmalıdır
        /// </remarks>
        [HttpPost("reprocess")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
                status = "reprocess-completed"
            });
        }
    }
}
