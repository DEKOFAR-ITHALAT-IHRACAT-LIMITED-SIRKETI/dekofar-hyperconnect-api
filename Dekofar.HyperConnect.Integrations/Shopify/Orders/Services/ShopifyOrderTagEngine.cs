using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;

public class ShopifyOrderTagEngine
{
    private readonly IEnumerable<IOrderTagRule> _rules;

    public ShopifyOrderTagEngine(IEnumerable<IOrderTagRule> rules)
    {
        _rules = rules;
    }

    public async Task<OrderTagResult?> CalculateAsync(
        JObject order,
        CancellationToken ct)
    {
        var results = new List<OrderTagResult>();

        foreach (var rule in _rules)
        {
            var result = await rule.EvaluateAsync(order, ct);
            if (result != null)
                results.Add(result);
        }

        if (!results.Any())
            return null;

        // =====================================================
        // 🔴 ARA1 VARSA → TEK KARAR
        // =====================================================
        var ara1Results = results
            .Where(r => r.Tag == "ara1")
            .OrderByDescending(r => r.Priority)
            .ToList();

        if (ara1Results.Any())
        {
            var selected = ara1Results.First();

            return new OrderTagResult
            {
                Tag = "ara1",
                Priority = selected.Priority,

                // SMS / otomasyon → TEK KOD
                ReasonCode = selected.ReasonCode,

                // İnsan → TÜM NOTLAR
                Notes = ara1Results
                    .SelectMany(r => r.Notes)
                    .Distinct()
                    .ToList()
            };
        }

        // =====================================================
        // 🟢 ARA1 YOKSA → EN YÜKSEK ÖNCELİK
        // =====================================================
        return results
            .OrderByDescending(r => r.Priority)
            .First();
    }
}
