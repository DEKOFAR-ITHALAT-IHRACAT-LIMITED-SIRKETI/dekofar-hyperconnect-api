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

    public Task<OrderTagResult?> EvaluateAsync(
        JObject order,
        CancellationToken ct)
    {
        var note =
            order["note"]?.ToString() ?? string.Empty;

        var address =
            order["shipping_address"]?["address1"]?.ToString() ?? string.Empty;

        var lineItemTitles =
            order["line_items"]?
                .Select(li => li["title"]?.ToString())
                .Where(t => !string.IsNullOrWhiteSpace(t))
            ?? Enumerable.Empty<string>();

        var fullText = string.Join(
                " ",
                note,
                address,
                string.Join(" ", lineItemTitles)
            )
            .ToLowerInvariant();

        var hit = ForbiddenWords.FirstOrDefault(w => fullText.Contains(w));

        if (hit == null)
            return Task.FromResult<OrderTagResult?>(null);

        var result = new OrderTagResult
        {
            Tag = "iptal",
            Priority = 1000, // 🔥 MUTLAK ÜSTÜNLÜK
            ReasonCode = "CANCEL_KEYWORD"
        };

        result.Notes.Add(
            $"Siparişte yasaklı kelime tespit edildi: '{hit}'");

        return Task.FromResult<OrderTagResult?>(result);
    }
}
