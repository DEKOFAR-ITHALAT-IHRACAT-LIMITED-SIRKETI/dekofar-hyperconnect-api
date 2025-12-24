using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules
{
    public class MultiProductRule : IOrderTagRule
    {
        public Task<IEnumerable<string>> EvaluateAsync(JObject order, CancellationToken ct)
        {
            var distinctProducts =
                order["line_items"]?
                    .Select(li => li["product_id"]?.ToString())
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Distinct()
                    .Count() ?? 0;

            return distinctProducts > 1
                ? Task.FromResult<IEnumerable<string>>(new[] { "ara1" })
                : Task.FromResult(Enumerable.Empty<string>());
        }
    }
}
