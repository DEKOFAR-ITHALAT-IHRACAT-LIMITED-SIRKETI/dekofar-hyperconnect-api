using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Models.Shopify.Dto
{
    public class ShopifyOrderLiteDto
    {
        public long Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string TotalPrice { get; set; } = string.Empty;
        public string Currency { get; set; } = "TRY";

        public string Status { get; set; } = "unknown"; // "Tamamlandı" vs.
        public string FinancialStatus { get; set; } = string.Empty;
        public string FulfillmentStatus { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public int CustomerOrderCount { get; set; }

        public int ItemCount { get; set; }
    }


}
