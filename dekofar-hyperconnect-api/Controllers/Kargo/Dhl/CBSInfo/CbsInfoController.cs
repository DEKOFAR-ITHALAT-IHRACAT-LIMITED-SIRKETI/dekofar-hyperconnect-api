using Dekofar.HyperConnect.Integrations.Kargo.Dhl.CBSInfo.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.CBSInfo.Models;
using Microsoft.AspNetCore.Mvc;

namespace dekofar_hyperconnect_api.Controllers.Kargo.Dhl.CBSInfo
{
    [ApiController]
    [Route("api/kargo/mng/cbsinfo")]
    public class CbsInfoController : ControllerBase
    {
        private readonly ICbsInfoService _cbsInfoService;

        public CbsInfoController(ICbsInfoService cbsInfoService)
        {
            _cbsInfoService = cbsInfoService;
        }

        [HttpGet("cities")]
        [ProducesResponseType(typeof(List<CityResponse>), 200)]
        public async Task<IActionResult> GetCities()
        {
            var result = await _cbsInfoService.GetCitiesAsync();
            return Ok(result);
        }
        [HttpGet("districts/{cityCode}")]
        [ProducesResponseType(typeof(List<DistrictResponse>), 200)]
        public async Task<IActionResult> GetDistrictsByCityCode(string cityCode)
        {
            var result = await _cbsInfoService.GetDistrictsByCityCodeAsync(cityCode);
            return Ok(result);
        }
        [HttpGet("neighborhoods/{cityCode}/{districtCode}")]
        [ProducesResponseType(typeof(List<NeighborhoodResponse>), 200)]
        public async Task<IActionResult> GetNeighborhoods(string cityCode, string districtCode)
        {
            var result = await _cbsInfoService.GetNeighborhoodsAsync(cityCode, districtCode);
            return Ok(result);
        }

        [HttpGet("outofserviceareas/{cityCode}/{districtCode}")]
        [ProducesResponseType(typeof(List<NeighborhoodResponse>), 200)]
        public async Task<IActionResult> GetOutOfServiceAreas(string cityCode, string districtCode)
        {
            var result = await _cbsInfoService.GetOutOfServiceAreasAsync(cityCode, districtCode);
            return Ok(result);
        }

        [HttpGet("mobileareas/{cityCode}/{districtCode}")]
        [ProducesResponseType(typeof(List<NeighborhoodResponse>), 200)]
        public async Task<IActionResult> GetMobileAreas(string cityCode, string districtCode)
        {
            var result = await _cbsInfoService.GetMobileAreasAsync(cityCode, districtCode);
            return Ok(result);
        }

    }
}
