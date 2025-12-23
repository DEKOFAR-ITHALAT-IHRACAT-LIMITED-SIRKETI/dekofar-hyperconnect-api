using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules
{
    /// <summary>
    /// DHL kargo için uygunluk kontrolü
    /// </summary>
    public class DhlRule : IOrderTagRule
    {
        private const int MaxWeightGrams = 20000; // 20 kg

        public Task<IEnumerable<string>> EvaluateAsync(
            JObject order,
            CancellationToken ct)
        {
            var address = order["shipping_address"];
            if (address == null)
                return Task.FromResult(Enumerable.Empty<string>());

            var countryCode = address["country_code"]?.ToString();
            if (string.IsNullOrWhiteSpace(countryCode))
                return Task.FromResult(Enumerable.Empty<string>());

            // TR dışı siparişler
            if (countryCode.Equals("TR", System.StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(Enumerable.Empty<string>());

            var totalWeight = order["total_weight"]?.Value<int>() ?? 0;
            if (totalWeight <= 0 || totalWeight > MaxWeightGrams)
                return Task.FromResult(Enumerable.Empty<string>());

            return Task.FromResult<IEnumerable<string>>(new[]
            {
                "DHL"
            });
        }
    }
}
