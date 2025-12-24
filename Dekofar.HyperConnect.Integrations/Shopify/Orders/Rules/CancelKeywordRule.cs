using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class CancelKeywordRule : IOrderTagRule
{
    private static readonly string[] ForbiddenWords =
    {
        "iptal", "deneme", "test", "sahte", "fake"
    };

    public Task<OrderTagResult?> EvaluateAsync(JObject order, CancellationToken ct)
    {
        var text = string.Join(" ",
            order["note"]?.ToString(),
            order["shipping_address"]?["address1"]?.ToString(),
            order["line_items"]?.Select(i => i["title"]?.ToString())
        ).ToLowerInvariant();

        var hit = ForbiddenWords.FirstOrDefault(w => text.Contains(w));

        if (hit != null)
        {
            return Task.FromResult<OrderTagResult?>(new OrderTagResult
            {
                Tag = "iptal",
                Reason = $"Yasaklı kelime bulundu: {hit}"
            });
        }

        return Task.FromResult<OrderTagResult?>(null);
    }
}
