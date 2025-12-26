using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;
using Newtonsoft.Json.Linq;

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
                Reason = "Tekrar sipariş veren müşteri",
                Priority = 90,
                Note = "Müşteri daha önce sipariş vermiş"
            });
        }

        return Task.FromResult<OrderTagResult?>(null);
    }
}
