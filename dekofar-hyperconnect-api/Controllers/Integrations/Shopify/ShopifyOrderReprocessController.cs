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
    /// TÜM açık + ödeme bekleyen + gönderilmemiş siparişleri
    /// baştan etiketler.
    /// 
    /// - Eski etiketleri siler
    /// - Kurallara göre TEK yeni etiket atar
    /// - ARA1 ise tüm sebepleri sistem notu olarak ekler
    /// </summary>
    [HttpPost("open-orders")]
    public async Task<IActionResult> ReprocessOpenOrders(
        CancellationToken ct)
    {
        var count = await _service.ReprocessOpenOrdersAsync(ct);
        return Ok(new { processed = count });
    }

    /// <summary>


}
