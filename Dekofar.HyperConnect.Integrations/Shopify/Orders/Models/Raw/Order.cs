using System.Text.Json.Serialization;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Models.Raw
{
    public class ShopifyOrdersResponse
    {
        [JsonPropertyName("orders")]
        public List<Order>? Orders { get; set; }
    }

    public class Order
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("tags")]
        public string? Tags { get; set; }

        // 🔥 KRİTİK
        [JsonPropertyName("line_items")]
        public List<LineItem>? LineItems { get; set; }
    }

    public class LineItem
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("variant_title")]
        public string? VariantTitle { get; set; }

        [JsonPropertyName("sku")]
        public string? Sku { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}
