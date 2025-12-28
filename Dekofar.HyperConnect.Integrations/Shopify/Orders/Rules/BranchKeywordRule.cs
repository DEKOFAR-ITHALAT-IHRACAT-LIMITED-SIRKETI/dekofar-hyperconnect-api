using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class BranchKeywordRule : IOrderTagRule
{
    private static readonly string[] Keywords =
    {
        "şube",
        "sube",
        "kargo şubesi",
        "teslim al"
    };

    public Task<OrderTagResult?> EvaluateAsync(JObject order, CancellationToken ct)
    {
        var address =
            order["shipping_address"]?["address1"]?
                .ToString()?.ToLowerInvariant() ?? "";

        if (Keywords.Any(k => address.Contains(k)))
        {
            var r = new OrderTagResult
            {
                Tag = "ara1",
                Priority = 90
            };

            r.Reasons.Add("Adres kargo şubesi / teslim noktası içeriyor");
            r.Notes.Add("Sipariş adresi şube veya teslim alma noktası");

            return Task.FromResult<OrderTagResult?>(r);
        }

        return Task.FromResult<OrderTagResult?>(null);
    }
}
