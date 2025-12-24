using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class ShippingDecisionRule : IOrderTagRule
{
    private static readonly string[] VillageKeywords =
    {
        "köy", "köyü", "mezra"
    };

    public Task<IEnumerable<string>> EvaluateAsync(JObject order, CancellationToken ct)
    {
        var address =
            order["shipping_address"]?["address1"]?.ToString()?.ToLowerInvariant() ?? "";

        var phone =
            order["shipping_address"]?["phone"]?.ToString();

        if (string.IsNullOrWhiteSpace(phone) || address.Length < 10)
            return Task.FromResult(Enumerable.Empty<string>());

        if (VillageKeywords.Any(k => address.Contains(k)))
            return Task.FromResult<IEnumerable<string>>(new[] { "ptt" });

        return Task.FromResult<IEnumerable<string>>(new[] { "dhl" });
    }
}
