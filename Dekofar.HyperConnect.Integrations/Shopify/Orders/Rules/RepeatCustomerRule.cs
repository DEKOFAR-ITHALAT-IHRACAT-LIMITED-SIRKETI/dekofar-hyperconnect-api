using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules
{
    /// <summary>
    /// Müşterinin daha önce sipariş verip vermediğini kontrol eder
    /// </summary>
    public class RepeatCustomerRule : IOrderTagRule
    {
        public Task<IEnumerable<string>> EvaluateAsync(
            JObject order,
            CancellationToken ct)
        {
            var ordersCount =
                order["customer"]?["orders_count"]?.Value<int>() ?? 0;

            // İlk sipariş = 1
            if (ordersCount > 1)
            {
                return Task.FromResult<IEnumerable<string>>(new[]
                {
                    "TEKRAR_SIPARIS"
                });
            }

            return Task.FromResult(Enumerable.Empty<string>());
        }
    }
}
