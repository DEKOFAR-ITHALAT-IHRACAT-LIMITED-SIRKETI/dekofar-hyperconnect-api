using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dekofar.HyperConnect.Integrations.Meta.Interfaces;
using Dekofar.HyperConnect.Integrations.Meta.Models;
using Microsoft.AspNetCore.Mvc;

namespace dekofar_hyperconnect_api.Controllers.Integrations.Meta
{
    [ApiController]
    [Route("api/integrations/meta/ads")]
    public class MetaAdsController : ControllerBase
    {
        private readonly IFacebookMarketingApiClient _fbClient;

        public MetaAdsController(IFacebookMarketingApiClient fbClient)
        {
            _fbClient = fbClient;
        }

        /// <summary>
        /// TEST için: adAccountId ve accessToken'ı query'den alıyoruz.
        /// Sonra DB'ye taşırsın.
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<FacebookAdDto>>> GetActiveAds(
            [FromQuery] string adAccountId,
            [FromQuery] string accessToken,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(adAccountId) || string.IsNullOrWhiteSpace(accessToken))
                return BadRequest("adAccountId ve accessToken zorunludur.");

            var ads = await _fbClient.GetActiveAdsAsync(adAccountId, accessToken, cancellationToken);

            return Ok(ads);
        }
    }
}
