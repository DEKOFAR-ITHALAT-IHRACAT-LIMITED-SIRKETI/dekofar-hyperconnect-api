using Newtonsoft.Json.Linq;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public interface IOrderTagRule
{
    Task<OrderTagResult?> EvaluateAsync(
        JObject order,
        CancellationToken ct);
}
