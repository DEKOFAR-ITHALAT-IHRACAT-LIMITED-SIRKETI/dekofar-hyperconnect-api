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

<<<<<<< HEAD
        // 🔴 1000 TL altı → ARA1
=======
        // 🔴 1000 TL ALTINDA → KESİN ARA1
>>>>>>> a785abc0de3cf57a56594c49b3f003c4f3dd90d2
        if (total < 1000)
        {
            return Task.FromResult<OrderTagResult?>(new OrderTagResult
            {
                Tag = "ara1",
<<<<<<< HEAD
                Priority = 95,
                Note = "Sipariş tutarı 1000 TL altında"
            });
        }

        // 🔴 2000 TL ve üzeri → ARA1
=======
                Reason = "Sipariş tutarı 1000 TL altı",
                Priority = 110,
                Note = "1000 TL altı sipariş – manuel kontrol gerekli"
            });
        }

        // 🔴 2000 TL VE ÜZERİ → KESİN ARA1
>>>>>>> a785abc0de3cf57a56594c49b3f003c4f3dd90d2
        if (total >= 2000)
        {
            return Task.FromResult<OrderTagResult?>(new OrderTagResult
            {
                Tag = "ara1",
<<<<<<< HEAD
                Priority = 85,
                Note = "Sipariş tutarı 2000 TL ve üzeri"
=======
                Reason = "Sipariş tutarı 2000 TL ve üzeri",
                Priority = 100,
                Note = "Yüksek tutarlı sipariş (2000 TL+)"
>>>>>>> a785abc0de3cf57a56594c49b3f003c4f3dd90d2
            });
        }

        // 🟢 1000–1999 → diğer kurallar karar versin
        return Task.FromResult<OrderTagResult?>(null);
    }
}
