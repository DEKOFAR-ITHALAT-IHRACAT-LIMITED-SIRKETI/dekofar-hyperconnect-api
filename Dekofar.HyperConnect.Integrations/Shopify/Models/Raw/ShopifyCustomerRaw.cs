using Newtonsoft.Json;

namespace Dekofar.HyperConnect.Integrations.Shopify.Models.Raw
{
    public class ShopifyCustomerRaw
    {
        [JsonProperty("phone")]
        public string? Phone { get; set; }
    }
}
