using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Models;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.Services
{
    public class DhlKargoDeliveredShipmentService : IDhlKargoDeliveredShipmentService
    {
        private readonly IConfiguration _config;
        private readonly IDhlKargoAuthService _authService;

        public DhlKargoDeliveredShipmentService(IConfiguration config, IDhlKargoAuthService authService)
        {
            _config = config;
            _authService = authService;
        }

        public async Task<List<DeliveredShipmentResponse>> GetDeliveredShipmentsByDateAsync(DateTime startDate)
        {
            var token = await _authService.GetTokenAsync();

            var client = new RestClient(
                $"https://api.mngkargo.com.tr/mngapi/api/bulkqueryapi/getDeliveredShipment/{startDate:dd-MM-yyyy}"
            );
            var request = new RestRequest("", Method.Get);

            AddCommonHeaders(request, token.jwt);

            var response = await client.ExecuteAsync(request);

            // ❗ 404 durumunda boş liste dön → Exception atma
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<DeliveredShipmentResponse>();
            }

            if (!response.IsSuccessful)
                throw new Exception($"DHL Kargo delivered shipments hatası: {response.StatusCode} - {response.Content}");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (string.IsNullOrWhiteSpace(response.Content))
                return new List<DeliveredShipmentResponse>();

            if (response.Content.TrimStart().StartsWith("["))
            {
                return JsonSerializer.Deserialize<List<DeliveredShipmentResponse>>(response.Content, options)
                       ?? new List<DeliveredShipmentResponse>();
            }
            else
            {
                var single = JsonSerializer.Deserialize<DeliveredShipmentResponse>(response.Content, options);
                return single != null ? new List<DeliveredShipmentResponse> { single } : new List<DeliveredShipmentResponse>();
            }
        }


        private void AddCommonHeaders(RestRequest request, string jwt)
        {
            var clientId = _config["DhlKargo:ClientId"];
            var clientSecret = _config["DhlKargo:ClientSecret"];

            request.AddHeader("accept", "application/json");
            request.AddHeader("X-IBM-Client-Id", clientId);
            request.AddHeader("X-IBM-Client-Secret", clientSecret);
            request.AddHeader("x-api-version", "1.0");
            request.AddHeader("Authorization", $"Bearer {jwt}");
        }
    }
}
