using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class BranchKeywordRule : IOrderTagRule
{
    private static readonly string[] Keywords =
    {
        "şube", "sube", "kargo şubesi", "teslim al"
    };

    public Task<OrderTagResult?> EvaluateAsync(JObject order, CancellationToken ct)
    {
        var address =
            order["shipping_address"]?["address1"]?
                .ToString()?.ToLowerInvariant() ?? "";

        if (Keywords.Any(k => address.Contains(k)))
        {
            return Task.FromResult<OrderTagResult?>(new OrderTagResult
            {
                Tag = "ara1",
                Reason = "Adres kargo şubesi içeriyor"
            });
        }

        return Task.FromResult<OrderTagResult?>(null);
    }
}
