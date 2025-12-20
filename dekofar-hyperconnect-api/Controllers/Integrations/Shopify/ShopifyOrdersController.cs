using Dekofar.HyperConnect.Integrations.Shopify.Orders;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Microsoft.AspNetCore.Mvc;

namespace dekofar_hyperconnect_api.Controllers.Integrations.Shopify
{
    [ApiController]
    [Route("api/integrations/shopify/orders")]
    public class ShopifyOrdersController : ControllerBase
    {
        private readonly ShopifyOrderReportService _reportService;

        public ShopifyOrdersController(
            ShopifyOrderReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// 🔍 TEST
        /// Açık (open) siparişleri listeler
        /// Opsiyonel: tag filtresi
        ///
        /// Örnek:
        /// /api/integrations/shopify/orders/open
        /// /api/integrations/shopify/orders/open?tag=SMS_SENT
        /// /api/integrations/shopify/orders/open?tag=
        /// </summary>
        [HttpGet("open")]
        public async Task<IActionResult> GetOpenOrders(
            [FromQuery] string? tag,
            CancellationToken ct)
        {
            var filter = new OrderItemReportFilter
            {
                Tag = tag
            };

            var result = await _reportService
                .GetOpenOrdersByTagAsync(filter, ct);

            return Ok(new
            {
                Count = result.Count,
                Items = result
            });
        }

        /// <summary>
        /// 🔍 TEST
        /// Açık siparişlerden sadece ETİKETSİZ olanlar
        /// </summary>
        [HttpGet("open/no-tag")]
        public async Task<IActionResult> GetOpenOrdersWithoutTag(
            CancellationToken ct)
        {
            var filter = new OrderItemReportFilter
            {
                Tag = "" // -tag:*
            };

            var result = await _reportService
                .GetOpenOrdersByTagAsync(filter, ct);

            return Ok(new
            {
                Count = result.Count,
                Items = result
            });
        }

        /// <summary>
        /// 🔍 TEST
        /// Açık siparişlerden BELİRLİ TAG’e sahip olanlar
        /// </summary>
        [HttpGet("open/by-tag/{tag}")]
        public async Task<IActionResult> GetOpenOrdersByTag(
            [FromRoute] string tag,
            CancellationToken ct)
        {
            var filter = new OrderItemReportFilter
            {
                Tag = tag
            };

            var result = await _reportService
                .GetOpenOrdersByTagAsync(filter, ct);

            return Ok(new
            {
                Tag = tag,
                Count = result.Count,
                Items = result
            });
        }

        /// <summary>
        /// 🔍 TEST
        /// Sadece rapor motorunun ayakta olup olmadığını kontrol eder
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                Status = "OK",
                Service = "ShopifyOrderReportService",
                Time = DateTime.UtcNow
            });
        }
    }
}
