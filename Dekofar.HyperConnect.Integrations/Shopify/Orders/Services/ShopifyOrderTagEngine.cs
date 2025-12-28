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

        // 🔴 ARA1 VARSA → TÜM SEBEPLERİ VE NOTLARI BİRLEŞTİR
        var ara1Results = results
            .Where(r => r.Tag == "ara1")
            .ToList();

        if (ara1Results.Any())
        {
            var merged = new OrderTagResult
            {
                Tag = "ara1",
                Priority = ara1Results.Max(x => x.Priority)
            };

            // ✅ TÜM SEBEPLER
            foreach (var reason in ara1Results.SelectMany(x => x.Reasons))
                if (!merged.Reasons.Contains(reason))
                    merged.Reasons.Add(reason);

            // ✅ TÜM NOTLAR
            foreach (var note in ara1Results.SelectMany(x => x.Notes))
                if (!merged.Notes.Contains(note))
                    merged.Notes.Add(note);

            return merged;
        }

        // 🟢 ARA1 YOKSA → EN YÜKSEK ÖNCELİKLİ TEK SONUÇ
        return results
            .OrderByDescending(x => x.Priority)
            .First();
    }
}
