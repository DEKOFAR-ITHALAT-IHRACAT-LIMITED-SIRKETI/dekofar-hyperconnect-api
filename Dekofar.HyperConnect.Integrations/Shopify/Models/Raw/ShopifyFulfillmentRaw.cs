using Newtonsoft.Json;

namespace Dekofar.HyperConnect.Integrations.Shopify.Models.Raw
{
    public class ShopifyFulfillmentRaw
    {
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("tracking_numbers")]
        public List<string>? TrackingNumbers { get; set; }

        [JsonProperty("tracking_company")]
        public string? TrackingCompany { get; set; }

        [JsonProperty("tracking_url")]
        public string? TrackingUrl { get; set; }
    }
}
