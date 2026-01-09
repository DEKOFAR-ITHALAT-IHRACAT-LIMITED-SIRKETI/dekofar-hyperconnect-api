namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Models.Dtos
{
    public class OrderItemReportDto
    {
        public long OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public DateTime? OrderDate { get; set; }

        public string? ProductTitle { get; set; }
        public string? VariantTitle { get; set; }
        public string? Sku { get; set; }

        public int Quantity { get; set; }
        public List<string> OrderTags { get; set; } = new();
    }
}
