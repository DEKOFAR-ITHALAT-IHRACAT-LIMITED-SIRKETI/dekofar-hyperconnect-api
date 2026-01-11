using Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dekofar.HyperConnect.Api.Controllers.Integrations
{
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
        /// ✅ Açık siparişleri yeniden etiketler
        /// Aşama 1: Tüm etiketleri temizler
        /// Aşama 2: Kurallara göre yeniden etiketler
        /// </summary>
        /// <remarks>
        /// Shopify API limitlerine uygun şekilde
        /// 100'erli batch + cursor pagination kullanır
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
                status = "completed"
            });
        }
    }
}
