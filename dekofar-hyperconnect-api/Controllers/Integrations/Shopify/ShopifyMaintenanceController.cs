using Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dekofar.HyperConnect.Api.Controllers.Integrations.Shopify
{
    [ApiController]
    [Route("api/integrations/shopify")]
    public sealed class ShopifyMaintenanceController : ControllerBase
    {
        private readonly ShopifyOrderResetService _reset;
        private readonly ShopifyOrderReprocessService _reprocess;

        public ShopifyMaintenanceController(
            ShopifyOrderResetService reset,
            ShopifyOrderReprocessService reprocess)
        {
            _reset = reset;
            _reprocess = reprocess;
        }

        [HttpPost("reset-tags")]
        public async Task<IActionResult> Reset(string shop, CancellationToken ct)
            => Ok(new
            {
                shop,
                cleared = await _reset.ResetAllOpenOrderTagsAsync(shop, ct)
            });

        [HttpPost("reprocess")]
        public async Task<IActionResult> Reprocess(string shop, CancellationToken ct)
            => Ok(new
            {
                shop,
                processed = await _reprocess.ReprocessOpenOrdersAsync(shop, ct)
            });
    }
}
