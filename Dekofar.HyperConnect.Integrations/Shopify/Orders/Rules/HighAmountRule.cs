using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class HighAmountRule : IOrderTagRule
{
    public Task<OrderTagResult?> EvaluateAsync(JObject order, CancellationToken ct)
    {
        var total =
            decimal.TryParse(
                order["total_price"]?.ToString(),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var price)
                ? price
                : 0;

        if (total >= 3000)
        {
            return Task.FromResult<OrderTagResult?>(new OrderTagResult
            {
                Tag = "ara1",
                Reason = "Sipariş tutarı 3000 TL ve üzeri"
            });
        }

        return Task.FromResult<OrderTagResult?>(null);
    }
}
