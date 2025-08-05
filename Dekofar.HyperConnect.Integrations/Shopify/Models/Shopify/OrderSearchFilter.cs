using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Models.Shopify
{
    public class OrderSearchFilter
    {
        public string? Query { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public string? Status { get; set; } // financial_status veya fulfillment_status gibi
        public int? Limit { get; set; } = 50;
    }
}
