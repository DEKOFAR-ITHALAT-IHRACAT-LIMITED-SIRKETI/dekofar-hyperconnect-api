using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Auth.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Models;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Text.Json;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery
{
    public class DeliveredShipmentService : IDeliveredShipmentService
    {
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;

        public DeliveredShipmentService(IConfiguration config, IAuthService authService)
        {
            _config = config;
            _authService = authService;
        }

        public async Task<List<DeliveredShipmentResponse>> GetDeliveredShipmentsAsync(string startDate)
        {
            var tokenResponse = await _authService.GetTokenAsync();
            var token = $"Bearer {tokenResponse.jwt}";

            var client = new RestClient($"https://api.mngkargo.com.tr/mngapi/api/bulkqueryapi/getDeliveredShipment/{startDate}");
            var request = new RestRequest("", Method.Get);

            request.AddHeader("X-IBM-Client-Id", _config["DhlKargo:ClientId"]);
            request.AddHeader("X-IBM-Client-Secret", _config["DhlKargo:ClientSecret"]);
            request.AddHeader("Authorization", token);
            request.AddHeader("accept", "application/json");

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                throw new Exception($"DHL GetDeliveredShipment hatası: {response.StatusCode} - {response.Content}");

            return JsonSerializer.Deserialize<List<DeliveredShipmentResponse>>(
                response.Content!,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            )!;
        }
    }
}
