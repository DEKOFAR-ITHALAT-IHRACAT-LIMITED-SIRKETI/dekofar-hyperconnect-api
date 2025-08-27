namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.Auth.Interfaces
{
    using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Auth.Models;

    /// <summary>
    /// DHL kimlik doğrulama servisi.
    /// </summary>
    public interface IAuthService
    {
        Task<TokenResponse> GetTokenAsync();
    }
}
