using Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services
{
    /// <summary>
    /// Shopify siparişi için tüm etiket kurallarını çalıştıran motor
    /// </summary>
    public class ShopifyOrderTagEngine
    {
        private readonly IEnumerable<IOrderTagRule> _rules;

        public ShopifyOrderTagEngine(IEnumerable<IOrderTagRule> rules)
        {
            _rules = rules;
        }

        /// <summary>
        /// Sipariş payload'ına göre eklenecek etiketleri hesaplar
        /// </summary>
        public async Task<List<string>> CalculateAsync(
            JObject order,
            CancellationToken ct)
        {
            var tags = new HashSet<string>(
                System.StringComparer.OrdinalIgnoreCase);

            foreach (var rule in _rules)
            {
                var ruleTags = await rule.EvaluateAsync(order, ct);

                foreach (var tag in ruleTags)
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                        tags.Add(tag.Trim());
                }
            }

            return new List<string>(tags);
        }
    }
}
