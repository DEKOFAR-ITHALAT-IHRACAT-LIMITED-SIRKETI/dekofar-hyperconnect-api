using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using System.Collections.Generic;

namespace Dekofar.HyperConnect.Integrations.Shopify.Common
{
    /// <summary>
    /// Shopify search query string builder
    /// </summary>
    public static class ShopifyQueryBuilder
    {
        /// <summary>
        /// OrderItemReportFilter nesnesinden
        /// Shopify search query üretir.
        /// </summary>
        public static string Build(OrderItemReportFilter filter)
        {
            var parts = new List<string>
            {
                "status:open"
            };

            if (filter == null)
                return string.Join(" ", parts);

            // Tag filtresi
            if (filter.Tag != null)
            {
                if (filter.Tag == "")
                    parts.Add("-tag:*");        // etiketsizler
                else
                    parts.Add($"tag:{filter.Tag}");
            }

            // Finansal durum
            if (!string.IsNullOrWhiteSpace(filter.FinancialStatusCsv))
                parts.Add($"financial_status:{filter.FinancialStatusCsv}");

            // Fulfillment durum
            if (!string.IsNullOrWhiteSpace(filter.FulfillmentStatusCsv))
                parts.Add($"fulfillment_status:{filter.FulfillmentStatusCsv}");

            return string.Join(" ", parts);
        }
    }
}
