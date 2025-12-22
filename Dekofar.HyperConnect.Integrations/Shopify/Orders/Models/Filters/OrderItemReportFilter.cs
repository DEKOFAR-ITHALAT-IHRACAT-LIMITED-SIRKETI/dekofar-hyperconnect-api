using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Models.Filters
{
    public class OrderItemReportFilter
    {
        public string? Tag { get; set; }
        public string? FinancialStatusCsv { get; set; }
        public string? FulfillmentStatusCsv { get; set; }
    }
}
