using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class HighAmountRule : IOrderTagRule
{
    public Task<IEnumerable<string>> EvaluateAsync(JObject order, CancellationToken ct)
    {
        var total =
            decimal.TryParse(
                order["total_price"]?.ToString(),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var price)
                ? price
                : 0;

        return total >= 3000
            ? Task.FromResult<IEnumerable<string>>(new[] { "ara1" })
            : Task.FromResult(Enumerable.Empty<string>());
    }
}
