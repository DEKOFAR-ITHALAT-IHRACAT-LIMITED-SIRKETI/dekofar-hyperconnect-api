using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Http;

namespace Dekofar.HyperConnect.Integrations.Shopify.OAuth;

public class ShopifyOAuthService
{
    private readonly IHttpClientFactory _http;

    public ShopifyOAuthService(IHttpClientFactory http)
    {
        _http = http;
    }

    public async Task<ShopifyTokenResponse> ExchangeCodeAsync(
        string shop,
        string code,
        string clientId,
        string clientSecret)
    {
        var client = _http.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"https://{shop}/admin/oauth/access_token",
            new
            {
                client_id = clientId,
                client_secret = clientSecret,
                code
            });

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ShopifyTokenResponse>())!;
    }
}
