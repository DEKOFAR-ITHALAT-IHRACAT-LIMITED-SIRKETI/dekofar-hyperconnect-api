namespace Dekofar.HyperConnect.Domain.Entities
{
    public class ShopifyWebhookEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Topic { get; set; } = null!;

        // Shopify order id / fulfillment id
        public string? ExternalId { get; set; }

        // RAW JSON
        public string Payload { get; set; } = null!;

        public DateTime CreatedAtUtc { get; set; }
    }
}
