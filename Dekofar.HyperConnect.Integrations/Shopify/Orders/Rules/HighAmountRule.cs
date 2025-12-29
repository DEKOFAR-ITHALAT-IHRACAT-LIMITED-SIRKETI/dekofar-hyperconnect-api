using Newtonsoft.Json.Linq;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using System.Globalization;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class HighAmountRule : IOrderTagRule
{
    public Task<OrderTagResult?> EvaluateAsync(
        JObject order,
        CancellationToken ct)
    {
        decimal.TryParse(
            order["total_price"]?.ToString(),
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out var total);

        if (total < 1000)
        {
            return Task.FromResult<OrderTagResult?>(new OrderTagResult
            {
                Tag = "ara1",
                Priority = 100,
                ReasonCode = "LOW_ORDER_AMOUNT",
                Notes =
                {
                    "Sipariş tutarı 1000 TL altında"
                }
            });
        }

        if (total >= 2000)
        {
            return Task.FromResult<OrderTagResult?>(new OrderTagResult
            {
                Tag = "ara1",
                Priority = 90,
                ReasonCode = "HIGH_ORDER_AMOUNT",
                Notes =
                {
                    "Sipariş tutarı 2000 TL ve üzeri"
                }
            });
        }

        return Task.FromResult<OrderTagResult?>(null);
    }
}
