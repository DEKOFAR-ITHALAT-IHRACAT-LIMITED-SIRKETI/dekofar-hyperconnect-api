using Dekofar.HyperConnect.Integrations.DHLKargo.Services;

namespace Dekofar.HyperConnect.Integrations.DHLKargo.Interfaces
{
    public interface IDhlKargoAuthService
    {
        Task<TokenResponse> GetTokenAsync();
    }
}
