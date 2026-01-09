using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Models.Shopify
{
    public class ShopifyOrderDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string FinancialStatus { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Currency { get; set; }
        public string? FulfillmentStatus { get; internal set; }
    }
}
