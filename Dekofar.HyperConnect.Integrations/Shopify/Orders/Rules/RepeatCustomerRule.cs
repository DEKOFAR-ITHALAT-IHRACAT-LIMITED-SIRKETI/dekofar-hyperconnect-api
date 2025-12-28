using Newtonsoft.Json.Linq;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class RepeatCustomerRule : IOrderTagRule
{
    public Task<OrderTagResult?> EvaluateAsync(
        JObject order,
        CancellationToken ct)
    {
        var ordersCount =
            order["customer"]?["orders_count"]?.Value<int>() ?? 0;

        // 🔴 2. sipariş ve üzeri → ARA1
        if (ordersCount > 1)
        {
            var result = new OrderTagResult
            {
                Tag = "ara1",
                Priority = 90
            };

            result.Reasons.Add("Tekrar sipariş veren müşteri");
            result.Notes.Add("Müşteri daha önce sipariş vermiş");

            return Task.FromResult<OrderTagResult?>(result);
        }

        return Task.FromResult<OrderTagResult?>(null);
    }
}
