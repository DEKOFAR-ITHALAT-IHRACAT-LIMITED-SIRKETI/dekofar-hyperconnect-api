using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class HighAmountRule : IOrderTagRule
{
    public Task<OrderTagResult?> EvaluateAsync(
        JObject order,
        CancellationToken ct)
    {
        var total =
            decimal.TryParse(
                order["total_price"]?.ToString(),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var price)
                ? price
                : 0;

        // 🔴 1000 TL altı → ARA1
        if (total < 1000)
        {
            return Task.FromResult<OrderTagResult?>(new OrderTagResult
            {
                Tag = "ara1",
                Priority = 95,
                Note = "Sipariş tutarı 1000 TL altında"
            });
        }

        // 🔴 2000 TL ve üzeri → ARA1
        if (total >= 2000)
        {
            return Task.FromResult<OrderTagResult?>(new OrderTagResult
            {
                Tag = "ara1",
                Priority = 85,
                Note = "Sipariş tutarı 2000 TL ve üzeri"
            });
        }

        return Task.FromResult<OrderTagResult?>(null);
    }
}
