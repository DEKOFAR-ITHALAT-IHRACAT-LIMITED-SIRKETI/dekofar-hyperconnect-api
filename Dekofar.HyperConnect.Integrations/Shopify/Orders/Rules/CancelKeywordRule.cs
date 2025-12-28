using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class CancelKeywordRule : IOrderTagRule
{
    private static readonly string[] ForbiddenWords =
    {
        "iptal",
        "deneme",
        "test",
        "sahte",
        "fake"
    };

    public Task<OrderTagResult?> EvaluateAsync(JObject order, CancellationToken ct)
    {
        var text = string.Join(" ",
                order["note"]?.ToString() ?? "",
                order["shipping_address"]?["address1"]?.ToString() ?? "",
                string.Join(" ",
                    order["line_items"]?
                        .Select(i => i["title"]?.ToString())
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                    ?? Enumerable.Empty<string>())
            )
            .ToLowerInvariant();

        var hit = ForbiddenWords.FirstOrDefault(w => text.Contains(w));

        if (hit != null)
        {
            var r = new OrderTagResult
            {
                Tag = "iptal",
                Priority = 1000 // 🔥 HER ŞEYİN ÜSTÜNDE
            };

            r.Reasons.Add($"Yasaklı kelime bulundu: {hit}");
            r.Notes.Add($"Sipariş içeriğinde '{hit}' kelimesi tespit edildi");

            return Task.FromResult<OrderTagResult?>(r);
        }

        return Task.FromResult<OrderTagResult?>(null);
    }
}
