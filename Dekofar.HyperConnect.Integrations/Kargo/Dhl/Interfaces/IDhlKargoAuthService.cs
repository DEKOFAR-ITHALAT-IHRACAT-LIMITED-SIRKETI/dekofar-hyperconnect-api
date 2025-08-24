using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Models;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.Interfaces
{
    public interface IDhlKargoAuthService
    {
        Task<TokenResponse> GetTokenAsync();
    }
}
