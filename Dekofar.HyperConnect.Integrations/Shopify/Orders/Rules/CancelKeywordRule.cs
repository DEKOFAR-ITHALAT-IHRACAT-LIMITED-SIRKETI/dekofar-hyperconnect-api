using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules
{
    public class CancelKeywordRule : IOrderTagRule
    {
        private static readonly string[] ForbiddenWords =
        {
        "iptal", "deneme", "test", "fake", "sahte", "dolandırıc"
    };

        public Task<IEnumerable<string>> EvaluateAsync(JObject order, CancellationToken ct)
        {
            var text = string.Join(" ",
                order["note"]?.ToString(),
                order["shipping_address"]?["address1"]?.ToString(),
                order["line_items"]?.Select(i => i["title"]?.ToString())
            ).ToLowerInvariant();

            if (ForbiddenWords.Any(w => text.Contains(w)))
                return Task.FromResult<IEnumerable<string>>(new[] { "iptal" });

            return Task.FromResult(Enumerable.Empty<string>());
        }
    }
}
