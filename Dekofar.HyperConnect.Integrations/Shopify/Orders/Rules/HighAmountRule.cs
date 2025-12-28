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

        // 🔴 1000 TL altı
        if (total < 1000)
        {
            var r = new OrderTagResult
            {
                Tag = "ara1",
                Priority = 95
            };
            r.Reasons.Add("Sipariş tutarı 1000 TL altında");
            r.Notes.Add("Kapıda ödeme için düşük tutar");
            return Task.FromResult<OrderTagResult?>(r);
        }

        // 🔴 2000 TL ve üzeri
        if (total >= 2000)
        {
            var r = new OrderTagResult
            {
                Tag = "ara1",
                Priority = 85
            };
            r.Reasons.Add("Sipariş tutarı 2000 TL ve üzeri");
            r.Notes.Add("Yüksek tutarlı sipariş");
            return Task.FromResult<OrderTagResult?>(r);
        }

        // 🟢 1000–1999 → sorun yok
        return Task.FromResult<OrderTagResult?>(null);
    }
}
