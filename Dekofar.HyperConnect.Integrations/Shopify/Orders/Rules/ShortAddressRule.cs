using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class ShortAddressRule : IOrderTagRule
{
    public Task<IEnumerable<string>> EvaluateAsync(JObject order, CancellationToken ct)
    {
        var address =
            order["shipping_address"]?["address1"]?.ToString() ?? "";

        return address.Length < 10
            ? Task.FromResult<IEnumerable<string>>(new[] { "ara1" })
            : Task.FromResult(Enumerable.Empty<string>());
    }
}
