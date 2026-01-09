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

        /// <summary>
        /// Açık siparişleri yeniden kurallara göre etiketler
        /// </summary>
        [HttpPost("reprocess")]
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
    }
}
