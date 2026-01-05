using Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;
using Microsoft.AspNetCore.Mvc;

namespace dekofar_hyperconnect_api.Controllers.Integrations.Shopify;

[ApiController]
[Route("api/integrations/shopify/orders/reprocess")]
public class ShopifyOrderReprocessController : ControllerBase
{
    private readonly ShopifyOrderReprocessService _service;

    public ShopifyOrderReprocessController(
        ShopifyOrderReprocessService service)
    {
        _service = service;
    }

    /// <summary>
    /// 🧪 TEST ENDPOINT
    /// 
    /// - SADECE SON 50 AÇIK SİPARİŞİ işler
    /// - Önce:
    ///   • Tüm etiketleri siler
    ///   • Sistem notlarını temizler
    /// - Sonra:
    ///   • Kurallara göre yeniden etiketler
    ///   • ara1 / dhl / iptal
    /// 
    /// ⚠️ Test amaçlıdır, prod’da batch limiti kaldırılabilir
    /// </summary>
    [HttpPost("open-orders")]
    public async Task<IActionResult> ReprocessOpenOrders(
        CancellationToken ct)
    {
        var processedCount =
            await _service.ReprocessOpenOrdersAsync(ct);

        return Ok(new
        {
            processed = processedCount,
            scope = "last_50_open_orders",
            status = "completed"
        });
    }
}
