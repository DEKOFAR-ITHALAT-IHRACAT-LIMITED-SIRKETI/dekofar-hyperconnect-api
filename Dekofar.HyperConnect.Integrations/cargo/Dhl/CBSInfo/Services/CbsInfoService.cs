using Dekofar.HyperConnect.Integrations.Kargo.Dhl.CBSInfo.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.CBSInfo.Models;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.CBSInfo.Services
{
    public class CbsInfoService : ICbsInfoService
    {
        private readonly IConfiguration _config;

        public CbsInfoService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<List<CityResponse>> GetCitiesAsync()
        {
            var clientId = _config["DhlKargo:ClientId"];
            var clientSecret = _config["DhlKargo:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                throw new Exception("ClientId veya ClientSecret appsettings.json içinde tanımlı değil!");

            var client = new RestClient("https://api.mngkargo.com.tr/mngapi/api/cbsinfoapi/getcities");
            var request = new RestRequest("", Method.Get);

            request.AddHeader("X-IBM-Client-Id", clientId);
            request.AddHeader("X-IBM-Client-Secret", clientSecret);
            request.AddHeader("x-api-version", "1.0"); // ✅ Versiyon
            request.AddHeader("accept", "application/json");

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                throw new Exception($"CBS Info API GetCities hatası: {response.StatusCode} - {response.Content}");

            var cities = JsonSerializer.Deserialize<List<CityResponse>>(
                response.Content!,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return cities ?? new List<CityResponse>();
        }
        public async Task<List<DistrictResponse>> GetDistrictsByCityCodeAsync(string cityCode)
        {
            var client = new RestClient($"https://api.mngkargo.com.tr/mngapi/api/cbsinfoapi/getdistricts/{cityCode}");
            var request = new RestRequest("", Method.Get);

            request.AddHeader("X-IBM-Client-Id", _config["DhlKargo:ClientId"]);
            request.AddHeader("X-IBM-Client-Secret", _config["DhlKargo:ClientSecret"]);
            request.AddHeader("accept", "application/json");

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new Exception($"DHL CBS GetDistricts hatası: {response.StatusCode} - {response.Content}");

            return JsonSerializer.Deserialize<List<DistrictResponse>>(response.Content!,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }
        public async Task<List<NeighborhoodResponse>> GetNeighborhoodsAsync(string cityCode, string districtCode)
        {
            var client = new RestClient($"https://api.mngkargo.com.tr/mngapi/api/cbsinfoapi/getneighborhoods/{cityCode}/{districtCode}");
            var request = new RestRequest("", Method.Get);

            request.AddHeader("X-IBM-Client-Id", _config["DhlKargo:ClientId"]);
            request.AddHeader("X-IBM-Client-Secret", _config["DhlKargo:ClientSecret"]);
            request.AddHeader("accept", "application/json");

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new Exception($"DHL CBS GetNeighborhoods hatası: {response.StatusCode} - {response.Content}");

            return JsonSerializer.Deserialize<List<NeighborhoodResponse>>(response.Content!,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }

        public async Task<List<NeighborhoodResponse>> GetOutOfServiceAreasAsync(string cityCode, string districtCode)
        {
            var client = new RestClient($"https://api.mngkargo.com.tr/mngapi/api/cbsinfoapi/getoutofserviceareas/{cityCode}/{districtCode}");
            var request = new RestRequest("", Method.Get);

            request.AddHeader("X-IBM-Client-Id", _config["DhlKargo:ClientId"]);
            request.AddHeader("X-IBM-Client-Secret", _config["DhlKargo:ClientSecret"]);
            request.AddHeader("accept", "application/json");

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new Exception($"DHL CBS GetOutOfServiceAreas hatası: {response.StatusCode} - {response.Content}");

            return JsonSerializer.Deserialize<List<NeighborhoodResponse>>(response.Content!,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }

        public async Task<List<NeighborhoodResponse>> GetMobileAreasAsync(string cityCode, string districtCode)
        {
            var client = new RestClient($"https://api.mngkargo.com.tr/mngapi/api/cbsinfoapi/getmobileareas/{cityCode}/{districtCode}");
            var request = new RestRequest("", Method.Get);

            request.AddHeader("X-IBM-Client-Id", _config["DhlKargo:ClientId"]);
            request.AddHeader("X-IBM-Client-Secret", _config["DhlKargo:ClientSecret"]);
            request.AddHeader("accept", "application/json");

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new Exception($"DHL CBS GetMobileAreas hatası: {response.StatusCode} - {response.Content}");

            return JsonSerializer.Deserialize<List<NeighborhoodResponse>>(response.Content!,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }

    }
}
