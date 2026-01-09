using Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dekofar.API.Controllers.Integrations
{
    [ApiController]
    [Route("api/integrations/shopify/reports")]
    public class ShopifyReportsController : ControllerBase
    {
        private readonly ShopifyOrderReportService _reportService;

        public ShopifyReportsController(
            ShopifyOrderReportService reportService)
        {
            _reportService = reportService;
        }

        // =====================================================
        // 📦 ÜRÜN / VARYANT ÖZET
        // =====================================================
        [HttpGet("orders/open/product-variant-summary")]
        public async Task<IActionResult> GetProductVariantSummary(
            [FromQuery] string shop,
            [FromQuery] string? tag,
            CancellationToken ct)
        {
            var result =
                await _reportService.GetOpenOrderProductSummaryAsync(
                    shop,
                    tag,
                    ct);

            return Ok(result);
        }

        // =====================================================
        // 🏷️ TAG ÖZET
        // =====================================================
        [HttpGet("orders/open/tag-summary")]
        public async Task<IActionResult> GetTagSummary(
            [FromQuery] string shop,
            CancellationToken ct)
        {
            var result =
                await _reportService.GetOpenOrderTagSummaryAsync(
                    shop,
                    ct);

            return Ok(result);
        }
    }
}
