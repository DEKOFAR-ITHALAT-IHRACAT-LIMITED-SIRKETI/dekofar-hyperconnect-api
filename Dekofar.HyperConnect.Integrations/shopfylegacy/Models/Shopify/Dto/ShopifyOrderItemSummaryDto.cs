using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Models.Shopify.Dto
{
    public class ShopifyOrderItemSummaryDto
    {
        public long ProductId { get; set; }
        public long? VariantId { get; set; }
        public string Title { get; set; } = "";
        public string? VariantTitle { get; set; }
        public int TotalQuantity { get; set; }
        public string? ImageUrl { get; set; }
    }
}
