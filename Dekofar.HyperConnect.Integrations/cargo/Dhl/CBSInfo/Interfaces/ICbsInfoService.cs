using Dekofar.HyperConnect.Integrations.Kargo.Dhl.CBSInfo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.CBSInfo.Interfaces
{
    public interface ICbsInfoService
    {
        Task<List<CityResponse>> GetCitiesAsync();
        Task<List<DistrictResponse>> GetDistrictsByCityCodeAsync(string cityCode);
        Task<List<NeighborhoodResponse>> GetNeighborhoodsAsync(string cityCode, string districtCode);
        Task<List<NeighborhoodResponse>> GetOutOfServiceAreasAsync(string cityCode, string districtCode);
        Task<List<NeighborhoodResponse>> GetMobileAreasAsync(string cityCode, string districtCode);
    }
}
