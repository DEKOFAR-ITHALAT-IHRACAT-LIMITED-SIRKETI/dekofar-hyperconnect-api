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
    /// Son 1 gün içindeki açık + ödeme bekleyen siparişleri
    /// kurallara göre yeniden etiketler
    /// </summary>
    [HttpPost("last-day")]
    public async Task<IActionResult> ReprocessLastDay(
        CancellationToken ct)
    {
        var count = await _service.ReprocessLastDayAsync(ct);
        return Ok(new { processed = count });
    }
}
