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

        return results
            .OrderByDescending(x => x.Priority)
            .FirstOrDefault();
    }
}
