using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.OAuth;

public class ShopifyTokenResponse
{
    public string access_token { get; set; } = null!;
    public string scope { get; set; } = null!;
}
