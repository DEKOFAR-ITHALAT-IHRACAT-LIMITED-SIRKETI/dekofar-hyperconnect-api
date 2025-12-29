using Newtonsoft.Json.Linq;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class RepeatCustomerRule : IOrderTagRule
{
    public Task<OrderTagResult?> EvaluateAsync(
        JObject order,
        CancellationToken ct)
    {
        var count =
            order["customer"]?["orders_count"]?.Value<int>() ?? 0;

        if (count > 1)
        {
            return Task.FromResult<OrderTagResult?>(new OrderTagResult
            {
                Tag = "ara1",
                Priority = 80,
                ReasonCode = "REPEAT_CUSTOMER",
                Notes =
                {
                    "Müşteri daha önce sipariş vermiş"
                }
            });
        }

        return Task.FromResult<OrderTagResult?>(null);
    }
}
