using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models.Filters;
using System.Collections.Generic;

namespace Dekofar.HyperConnect.Integrations.Shopify.Common
{
    /// <summary>
    /// Shopify Orders Search Query Builder
    /// NOT: Açık / kapalı / kargo durumu burada yapılmaz.
    /// Sadece basit tag filtreleri kullanılır.
    /// </summary>
    public static class ShopifyQueryBuilder
    {
        public static string Build(OrderItemReportFilter? filter)
        {
            var parts = new List<string>();

            if (filter?.Tag != null)
            {
                if (filter.Tag == "")
                {
                    // etiketsiz siparişler
                    parts.Add("-tag:*");
                }
                else
                {
                    parts.Add($"tag:{filter.Tag}");
                }
            }

            // boş string dönebilir → bu BİLİNÇLİ
            // tüm siparişleri almak için
            return string.Join(" ", parts);
        }
    }
}
