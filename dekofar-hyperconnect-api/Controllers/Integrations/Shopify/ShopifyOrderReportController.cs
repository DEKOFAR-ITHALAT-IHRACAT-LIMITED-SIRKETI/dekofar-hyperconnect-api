using Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;
using Microsoft.AspNetCore.Mvc;

namespace dekofar_hyperconnect_api.Controllers.Integrations.Shopify
{
    [ApiController]
    [Route("api/integrations/shopify/reports/orders")]
    public class ShopifyOrderReportController : ControllerBase
    {
        private readonly ShopifyOrderReportService _service;

        public ShopifyOrderReportController(
            ShopifyOrderReportService service)
        {
            _service = service;
        }

        // =====================================================
        // 🔹 AÇIK + GÖNDERİLMEMİŞ SİPARİŞLER
        // ÜRÜN → VARYANT → TOPLAM ADET (TAG OPSİYONEL)
        // =====================================================
        /// <summary>
        /// Açık ve gönderilmemiş siparişlerdeki ürünleri
        /// Ürün → Varyant → Toplam Adet şeklinde özetler.
        /// 
        /// Opsiyonel olarak ?tag=ONAY gibi bir etiket ile filtrelenebilir.
        /// </summary>
        /// <param name="tag">Shopify sipariş etiketi (opsiyonel)</param>
        /// <param name="ct">Cancellation token</param>
        [HttpGet("open/product-variant-summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOpenOrderSummary(
            [FromQuery] string? tag,
            CancellationToken ct)
        {
            var result =
                await _service.GetOpenOrderProductSummaryAsync(tag, ct);

            return Ok(result);
        }

        // =====================================================
        // 🔹 AÇIK SİPARİŞLER → ETİKET / SİPARİŞ SAYISI
        // =====================================================
        /// <summary>
        /// Açık siparişlerde kullanılan etiketlerin
        /// etiket → sipariş sayısı özetini döner.
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        [HttpGet("open/tag-summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOpenOrderTagSummary(
            CancellationToken ct)
        {
            var result =
                await _service.GetOpenOrderTagSummaryAsync(ct);

            return Ok(result);
        }
    }
}
