using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class MultiProductRule : IOrderTagRule
{
    public Task<OrderTagResult?> EvaluateAsync(
        JObject order,
        CancellationToken ct)
    {
        var distinctProducts =
            order["line_items"]?
                .Select(li => li["product_id"]?.ToString())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .Count() ?? 0;

        if (distinctProducts > 1)
        {
            return Task.FromResult<OrderTagResult?>(new OrderTagResult
            {
                Tag = "ara1",
                Priority = 80,

                // 🔑 SMS / otomasyon
                ReasonCode = "MULTI_PRODUCT",

                // 👤 İnsan
                Notes =
                {
                    "Siparişte birden fazla ürün çeşidi bulunuyor"
                }
            });
        }

        return Task.FromResult<OrderTagResult?>(null);
    }
}
