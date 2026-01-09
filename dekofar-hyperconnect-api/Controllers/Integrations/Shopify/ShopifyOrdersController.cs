using Dekofar.HyperConnect.Integrations.Shopify.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace dekofar_hyperconnect_api.Controllers.Integrations.Shopify
{
    [ApiController]
    [Route("api/integrations/shopify/orders")]
    public class ShopifyOrdersController : ControllerBase
    {
        private readonly IShopifyService _shopifyService;

        public ShopifyOrdersController(IShopifyService shopifyService)
        {
            _shopifyService = shopifyService;
        }

        /// <summary>
        /// Shopify mağazasındaki SON 10 siparişi getirir
        /// </summary>
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestOrders(
            [FromQuery] string shop,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(shop))
                return BadRequest("shop query parameter is required");

            var orders = await _shopifyService.GetLatestOrdersAsync(
                shopDomain: shop,
                limit: 10,
                ct: ct
            );

            return Ok(new
            {
                count = orders.Count,
                orders
            });
        }
    }
}
