using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Auth.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Models;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Services
{
    public class GetShipmentStatusByReferenceIdService : IGetShipmentStatusByReferenceIdService
    {
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;

        public GetShipmentStatusByReferenceIdService(IConfiguration config, IAuthService authService)
        {
            _config = config;
            _authService = authService;
        }

        public async Task<GetShipmentStatusResponse> GetShipmentStatusByReferenceIdAsync(string referenceId)
        {
            // 🔑 Önce token al
            var tokenResponse = await _authService.GetTokenAsync();
            var token = $"Bearer {tokenResponse.jwt}";

            // 📌 BaseUrl config’ten okunuyor
            var baseUrl = _config["DhlKargo:BaseUrl"];
            var client = new RestClient($"{baseUrl}/getshipmentstatus/{referenceId}");

            var request = BuildRequest(token);
            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                throw new Exception($"DHL GetShipmentStatusByReferenceId hatası: {response.StatusCode} - {response.Content}");

            return JsonSerializer.Deserialize<GetShipmentStatusResponse>(
                response.Content!,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            )!;
        }

        private RestRequest BuildRequest(string token)
        {
            var request = new RestRequest("", Method.Get);

            request.AddHeader("X-IBM-Client-Id", _config["DhlKargo:ClientId"]);
            request.AddHeader("X-IBM-Client-Secret", _config["DhlKargo:ClientSecret"]);
            request.AddHeader("x-api-version", _config["DhlKargo:ApiVersion"] ?? "1.0");
            request.AddHeader("Authorization", token);
            request.AddHeader("accept", "application/json");

            return request;
        }
    }
}
