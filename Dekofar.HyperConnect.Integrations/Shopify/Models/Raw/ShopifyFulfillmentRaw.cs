using Newtonsoft.Json;

namespace Dekofar.HyperConnect.Integrations.Shopify.Models.Raw
{
    public class ShopifyFulfillmentRaw
    {
        // ✅ FULFILLED TARİHİ BURADAN GELİR
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("tracking_numbers")]
        public List<string>? TrackingNumbers { get; set; }
    }
}
