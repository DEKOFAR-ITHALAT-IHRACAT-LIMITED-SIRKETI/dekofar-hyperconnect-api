using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Auth.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Models;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Globalization;
using System.Text.Json;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Services
{
    public class ShipmentByDateDetailService : IShipmentByDateDetailService
    {
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;

        public ShipmentByDateDetailService(IConfiguration config, IAuthService authService)
        {
            _config = config;
            _authService = authService;
        }

        public async Task<List<ShipmentByDateDetailResponse>> GetShipmentsByDateDetailAsync(string date)
        {
            var parsedDate = DateTime.Parse(date, new CultureInfo("tr-TR"));

            var tokenResponse = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(tokenResponse?.jwt))
                throw new Exception("DHL token alınamadı!");

            var token = $"Bearer {tokenResponse.jwt}";

            var client = new RestClient("https://api.mngkargo.com.tr/mngapi/api/bulkqueryapi/GetShipmentByDateDetail");
            var request = new RestRequest("", Method.Post);

            var clientId = _config["DhlKargo:ClientId"];
            var clientSecret = _config["DhlKargo:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                throw new Exception("DhlKargo ClientId veya ClientSecret appsettings.json içinde tanımlı değil!");

            request.AddHeader("X-IBM-Client-Id", clientId);
            request.AddHeader("X-IBM-Client-Secret", clientSecret);
            request.AddHeader("Authorization", token);
            request.AddHeader("accept", "application/json");
            request.AddHeader("Content-Type", "application/json-patch+json");

            // ✅ Body: date, reportType, subCompany
            request.AddStringBody(
                JsonSerializer.Serialize(new
                {
                    date = parsedDate.ToString("d/M/yyyy", CultureInfo.InvariantCulture), // "8/08/2025" formatı
                    reportType = 1,
                    subCompany = 1
                }),
                "application/json-patch+json"
            );

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                throw new Exception($"DHL GetShipmentByDateDetail hatası: {response.StatusCode} - {response.Content}");

            var shipments = JsonSerializer.Deserialize<List<ShipmentByDateDetailResponse>>(
                response.Content!,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return shipments ?? new List<ShipmentByDateDetailResponse>();
        }

    }
}
