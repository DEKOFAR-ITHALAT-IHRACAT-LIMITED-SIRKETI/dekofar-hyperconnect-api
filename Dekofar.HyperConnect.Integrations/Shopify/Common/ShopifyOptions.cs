using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Common
{
    public class ShopifyOptions
    {
        public string BaseUrl { get; set; } = default!;
        public string AccessToken { get; set; } = default!;
        public string WebhookSecret { get; set; } = default!;
    }
}
