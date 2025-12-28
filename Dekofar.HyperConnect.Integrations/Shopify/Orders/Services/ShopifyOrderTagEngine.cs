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

        // 🔴 ARA1 varsa → hepsini birleştir
        var ara1Results = results
            .Where(r => r.Tag == "ara1")
            .ToList();

        if (ara1Results.Any())
        {
            return new OrderTagResult
            {
                Tag = "ara1",
                Priority = ara1Results.Max(x => x.Priority),
                Reasons = ara1Results
                    .SelectMany(x => x.Reasons)
                    .Distinct()
                    .ToList()
            };
        }

        // 🟢 ARA1 yoksa en yüksek öncelik
        return results
            .OrderByDescending(x => x.Priority)
            .First();
    }
}
