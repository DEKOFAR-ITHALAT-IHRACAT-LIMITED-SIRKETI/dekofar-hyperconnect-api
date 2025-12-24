using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class BranchKeywordRule : IOrderTagRule
{
    private static readonly string[] Keywords =
    {
        "şube", "sube", "kargo şubesi", "teslim al"
    };

    public Task<IEnumerable<string>> EvaluateAsync(JObject order, CancellationToken ct)
    {
        var address =
            order["shipping_address"]?["address1"]?.ToString()?.ToLowerInvariant() ?? "";

        return Keywords.Any(k => address.Contains(k))
            ? Task.FromResult<IEnumerable<string>>(new[] { "ara1" })
            : Task.FromResult(Enumerable.Empty<string>());
    }
}
