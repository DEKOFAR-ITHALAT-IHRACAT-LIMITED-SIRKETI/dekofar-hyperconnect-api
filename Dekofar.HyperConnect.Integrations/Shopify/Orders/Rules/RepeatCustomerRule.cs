using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class RepeatCustomerRule : IOrderTagRule
{
    public Task<OrderTagResult?> EvaluateAsync(JObject order, CancellationToken ct)
    {
        var ordersCount =
            order["customer"]?["orders_count"]?.Value<int>() ?? 0;

        if (ordersCount > 1)
        {
            return Task.FromResult<OrderTagResult?>(new OrderTagResult
            {
                Tag = "ara1",
                Priority = 90,
                Reasons =
                {
                    $"Tekrar müşteri ({ordersCount}. sipariş)"
                }
            });
        }

        return Task.FromResult<OrderTagResult?>(null);
    }
}
