namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.Models
{
    public class TokenResponse
    {
        public string jwt { get; set; }
        public string refreshToken { get; set; }
        public string jwtExpireDate { get; set; }
        public string refreshTokenExpireDate { get; set; }
    }
}
