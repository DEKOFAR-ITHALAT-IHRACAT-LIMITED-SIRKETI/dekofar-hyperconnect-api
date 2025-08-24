using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Models;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.Services
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

            var clientId = _config["DhlKargo:ClientId"];
            var clientSecret = _config["DhlKargo:ClientSecret"];
            var customerNumber = _config["DhlKargo:CustomerNumber"];
            var password = _config["DhlKargo:Password"];

            request.AddHeader("accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-IBM-Client-Id", clientId);
            request.AddHeader("X-IBM-Client-Secret", clientSecret);

            var body = new
            {
                customerNumber,
                password,
                identityType = 1
            };
            request.AddJsonBody(body);

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                throw new Exception($"DHL Kargo token hatası: {response.StatusCode} - {response.Content}");

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(response.Content!);

            return tokenResponse!;
        }
    }
}
