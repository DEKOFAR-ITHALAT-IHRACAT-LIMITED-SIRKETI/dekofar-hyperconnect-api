using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Common
{
    public class ShopifyStore
    {
        public Guid Id { get; set; }
        public string ShopDomain { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string Scopes { get; set; } = null!;
        public DateTime InstalledAt { get; set; }
        public DateTime InstalledAtUtc { get; set; }
    }

}
