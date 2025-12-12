using Newtonsoft.Json;

namespace Dekofar.HyperConnect.Integrations.Shopify.Models.Raw
{
    public class ShopifyOrderRaw
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
        [JsonProperty("tags")]
        public string? Tags { get; set; }   // 👈 EKLENDİ

        // ✅ EKSİK OLAN BUYDU
        [JsonProperty("fulfillment_status")]
        public string? FulfillmentStatus { get; set; }

        [JsonProperty("customer")]
        public ShopifyCustomerRaw? Customer { get; set; }

        [JsonProperty("fulfillments")]
        public List<ShopifyFulfillmentRaw>? Fulfillments { get; set; }
    }
}
