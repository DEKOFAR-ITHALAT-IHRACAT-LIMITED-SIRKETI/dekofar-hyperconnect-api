using System;

namespace Dekofar.HyperConnect.Domain.Entities
{
    public class ShopifyStore
    {
        public Guid Id { get; set; }

        public string ShopDomain { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string Scopes { get; set; } = null!;

        // ⚠️ DB'de bu isim var
        public DateTime InstalledAtUtc { get; set; }
    }
}
