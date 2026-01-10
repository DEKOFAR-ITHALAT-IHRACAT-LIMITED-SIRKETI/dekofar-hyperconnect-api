using Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dekofar.HyperConnect.Api.Controllers.Integrations
{
    [ApiController]
    [Route("api/integrations/shopify")]
    public class ShopifyMaintenanceController : ControllerBase
    {
        private readonly ShopifyOrderReprocessService _reprocessService;

        public ShopifyMaintenanceController(
            ShopifyOrderReprocessService reprocessService)
        {
            _reprocessService = reprocessService;
        }

        // =====================================================
        // 🚀 PROD – TÜM AÇIK SİPARİŞLERİ YENİDEN ETİKETLER
        // =====================================================
        /// <summary>
        /// Açık siparişleri yeniden kurallara göre etiketler (PROD)
        /// </summary>
        [HttpPost("reprocess")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ReprocessOpenOrders(
            [FromQuery] string shop,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(shop))
                return BadRequest("shop query param required");

            var processed =
                await _reprocessService.ReprocessOpenOrdersAsync(
                    shop,
                    ct);

            return Ok(new
            {
                shop,
                processed
            });
        }

        // =====================================================
        // 🧪 TEST – SADECE İLK N SİPARİŞ (DEFAULT 10)
        // =====================================================
        /// <summary>
        /// Test amaçlı: sadece ilk N açık siparişi etiketler
        /// </summary>
        [HttpPost("reprocess/test")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ReprocessTestOrders(
            [FromQuery] string shop,
            [FromQuery] int limit = 10,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(shop))
                return BadRequest("shop query param required");

            if (limit <= 0 || limit > 50)
                return BadRequest("limit must be between 1 and 50");

            var processed =
                await _reprocessService.ReprocessOpenOrdersTestAsync(
                    shop,
                    limit,
                    ct);

            return Ok(new
            {
                shop,
                limit,
                processed
            });
        }
    }
}
