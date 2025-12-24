using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class ShippingDecisionRule : IOrderTagRule
{
    private static readonly string[] VillageKeywords =
    {
        "köy", "köyü", "mezra"
    };

    public Task<OrderTagResult?> EvaluateAsync(JObject order, CancellationToken ct)
    {
        var address =
            order["shipping_address"]?["address1"]?
                .ToString()?.ToLowerInvariant() ?? "";

        var phone =
            order["shipping_address"]?["phone"]?.ToString();

        // telefon yoksa veya adres kısa ise DHL/PTT kararı verilmez
        if (string.IsNullOrWhiteSpace(phone) || address.Length < 10)
            return Task.FromResult<OrderTagResult?>(null);

        if (VillageKeywords.Any(k => address.Contains(k)))
        {
            return Task.FromResult<OrderTagResult?>(new OrderTagResult
            {
                Tag = "ptt",
                Reason = "Adres köy/mezra içeriyor"
            });
        }

        return Task.FromResult<OrderTagResult?>(new OrderTagResult
        {
            Tag = "dhl",
            Reason = "Adres uygun, telefon mevcut"
        });
    }
}
