using Dekofar.HyperConnect.Integrations.DHLKargo.Interfaces;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Text.Json;

namespace Dekofar.HyperConnect.Integrations.DHLKargo.Services
{
    public class DhlKargoAuthService : IDhlKargoAuthService
    {
        private readonly IConfiguration _config;

        public DhlKargoAuthService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<TokenResponse> GetTokenAsync()
        {
            var client = new RestClient("https://api.mngkargo.com.tr/mngapi/api/token");
            var request = new RestRequest("", Method.Post);

            // Config’den okuma
            var clientId = _config["DhlKargo:ClientId"];
            var clientSecret = _config["DhlKargo:ClientSecret"];
            var customerNumber = _config["DhlKargo:CustomerNumber"];
            var password = _config["DhlKargo:Password"];

            // Headers
            request.AddHeader("accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-IBM-Client-Id", clientId);
            request.AddHeader("X-IBM-Client-Secret", clientSecret);

            // Body
            var body = new
            {
                customerNumber,
                password,
                identityType = 1
            };
            request.AddJsonBody(body);

            // Request
            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                throw new Exception($"DHL Kargo token hatası: {response.StatusCode} - {response.Content}");

            Console.WriteLine("RAW RESPONSE: " + response.Content);

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(response.Content!);

            return tokenResponse!;
        }
    }

    public class TokenResponse
    {
        public string jwt { get; set; }
        public string refreshToken { get; set; }
        public string jwtExpireDate { get; set; }
        public string refreshTokenExpireDate { get; set; }
    }
}
