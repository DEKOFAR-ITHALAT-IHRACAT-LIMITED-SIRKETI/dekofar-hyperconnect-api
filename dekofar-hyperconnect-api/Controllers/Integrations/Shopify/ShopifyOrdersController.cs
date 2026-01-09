using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/integrations/shopify/orders")]
public class ShopifyOrdersController : ControllerBase
{
    private readonly ShopifyService _shopifyService;

    public ShopifyOrdersController(ShopifyService shopifyService)
    {
        _shopifyService = shopifyService;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestOrders(
        [FromQuery] string shop,
        CancellationToken ct)
    {
        var orders = await _shopifyService.GetLatestOrdersAsync(
            shop,
            10,
            ct
        );

        return Ok(new
        {
            count = orders.Count,
            orders
        });
    }

    [HttpPost("create")]
    public IActionResult Create()
    {
        return Ok(new { message = "Webhook endpoint hazır" });
    }
}
