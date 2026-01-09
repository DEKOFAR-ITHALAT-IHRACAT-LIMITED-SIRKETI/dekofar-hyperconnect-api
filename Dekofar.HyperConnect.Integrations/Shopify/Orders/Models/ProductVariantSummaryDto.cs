namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Models
{
    public class ProductVariantSummaryDto
    {
        public string ProductTitle { get; set; } = default!;
        public List<VariantSummaryDto> Variants { get; set; } = new();
        public string? ProductImageUrl { get; set; }


        public int TotalQuantity => Variants.Sum(v => v.Quantity);
    }

    public class VariantSummaryDto
    {
        public string VariantTitle { get; set; } = default!;
        public string? Sku { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }

    }
}
