using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class ShortAddressRule : IOrderTagRule
{
    public Task<OrderTagResult?> EvaluateAsync(
        JObject order,
        CancellationToken ct)
    {
        var address =
            order["shipping_address"]?["address1"]?
                .ToString() ?? "";

        if (address.Length < 10)
        {
            return Task.FromResult<OrderTagResult?>(new OrderTagResult
            {
                Tag = "ara1",
                Priority = 95,

                // 🔑 SMS / otomasyon
                ReasonCode = "SHORT_ADDRESS",

                // 👤 İnsan (Shopify note)
                Notes =
                {
                    "Adres uzunluğu 10 karakterden kısa"
                }
            });
        }

        return Task.FromResult<OrderTagResult?>(null);
    }
}
