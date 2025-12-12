using Newtonsoft.Json;

namespace Dekofar.HyperConnect.Integrations.Shopify.Models.Raw
{
    public class ShopifyOrderRawResponse
    {
        [JsonProperty("orders")]
        public List<ShopifyOrderRaw> Orders { get; set; } = new();
    }
}
