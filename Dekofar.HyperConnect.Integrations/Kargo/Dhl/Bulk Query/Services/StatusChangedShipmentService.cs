using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Auth.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Models;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Globalization;
using System.Text.Json;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Services
{
    public class StatusChangedShipmentService : IStatusChangedShipmentService
    {
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;

        public StatusChangedShipmentService(IConfiguration config, IAuthService authService)
        {
            _config = config;
            _authService = authService;
        }

        public async Task<List<StatusChangedShipmentResponse>> GetStatusChangedShipmentsAsync(string startDate, string endDate)
        {
            var decodedStart = Uri.UnescapeDataString(startDate);
            var decodedEnd = Uri.UnescapeDataString(endDate);

            string formattedStart, formattedEnd;

            try
            {
                var parsedStart = DateTime.Parse(decodedStart, new CultureInfo("tr-TR"));
                formattedStart = parsedStart.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new Exception($"Geçersiz başlangıç tarihi: {decodedStart}. Beklenen format: dd.MM.yyyy");
            }

            try
            {
                var parsedEnd = DateTime.Parse(decodedEnd, new CultureInfo("tr-TR"));
                formattedEnd = parsedEnd.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new Exception($"Geçersiz bitiş tarihi: {decodedEnd}. Beklenen format: dd.MM.yyyy HH:mm:ss");
            }

            var tokenResponse = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(tokenResponse?.jwt))
                throw new Exception("DHL token alınamadı!");

            var token = $"Bearer {tokenResponse.jwt}";

            var url = $"https://api.mngkargo.com.tr/mngapi/api/bulkqueryapi/getStatusChangedShipments/{formattedStart}/{formattedEnd}";
            var client = new RestClient(url);
            var request = new RestRequest("", Method.Get);

            var clientId = _config["DhlKargo:ClientId"];
            var clientSecret = _config["DhlKargo:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                throw new Exception("DhlKargo ClientId veya ClientSecret appsettings.json içinde tanımlı değil!");

            request.AddHeader("X-IBM-Client-Id", clientId);
            request.AddHeader("X-IBM-Client-Secret", clientSecret);
            request.AddHeader("Authorization", token);
            request.AddHeader("accept", "application/json");

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                throw new Exception($"DHL GetStatusChangedShipments hatası: {response.StatusCode} - {response.Content}");

            var shipments = JsonSerializer.Deserialize<List<StatusChangedShipmentResponse>>(
                response.Content!,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return shipments ?? new List<StatusChangedShipmentResponse>();
        }
    }
}
