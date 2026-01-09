using Dekofar.HyperConnect.Integrations.Shopify.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace dekofar_hyperconnect_api.Controllers.Integrations.Shopify
{
    [ApiController]
    [Route("api/integrations/shopify/orders")]
    public class ShopifyOrdersController : ControllerBase
    {
        private readonly ShopifyService _shopifyService;

        public ShopifyOrdersController(ShopifyService shopifyService)
        {
            _shopifyService = shopifyService;
        }

        /// <summary>
        /// Shopify mağazasındaki SON 10 siparişi getirir
        /// </summary>
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestOrders(CancellationToken ct)
        {
            // pageInfo = null → ilk sayfa
            // limit = 10
            var result = await _shopifyService.GetOrdersPagedAsync(
                pageInfo: null,
                limit: 10,
                ct: ct
            );

            return Ok(new
            {
                count = result.Items.Count,
                orders = result.Items
            });
        }
    }
}
