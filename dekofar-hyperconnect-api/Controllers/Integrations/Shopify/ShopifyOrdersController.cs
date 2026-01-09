using Dekofar.HyperConnect.Application.Integrations.Shopify.Services;
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
        /// <remarks>
        /// Aktif ShopifyStore kaydı olan mağazadan verileri çeker
        /// </remarks>
        [HttpGet("latest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLatestOrders(CancellationToken ct)
        {
            var result = await _shopifyService.GetOrdersPagedAsync(
                pageInfo: null, // ilk sayfa
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
