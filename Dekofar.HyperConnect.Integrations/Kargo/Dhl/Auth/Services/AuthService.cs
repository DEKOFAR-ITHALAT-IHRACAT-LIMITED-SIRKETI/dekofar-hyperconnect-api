namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.Auth.Services
{
    using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Auth.Interfaces;
    using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Auth.Models;
    using Microsoft.Extensions.Configuration;
    using RestSharp;
    using System.Text.Json;

    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;

        public AuthService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<TokenResponse> GetTokenAsync()
        {
            var client = new RestClient("https://api.mngkargo.com.tr/mngapi/api/token");
            var request = new RestRequest("", Method.Post);

            request.AddHeader("accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-IBM-Client-Id", _config["DhlKargo:ClientId"]);
            request.AddHeader("X-IBM-Client-Secret", _config["DhlKargo:ClientSecret"]);

            var body = new
            {
                customerNumber = _config["DhlKargo:CustomerNumber"],
                password = _config["DhlKargo:Password"],
                identityType = 1
            };
            request.AddJsonBody(body);

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                throw new Exception($"DHL Token hatası: {response.StatusCode} - {response.Content}");

            return JsonSerializer.Deserialize<TokenResponse>(
                response.Content!,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            )!;
        }
    }
}
