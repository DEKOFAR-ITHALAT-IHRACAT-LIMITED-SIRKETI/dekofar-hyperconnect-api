using Newtonsoft.Json.Linq;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class ShippingDecisionRule : IOrderTagRule
{
    private static readonly string[] WeakKeywords =
    {
        "avm","şube","kargo","teslim al","hastane"
    };

    public Task<OrderTagResult?> EvaluateAsync(
        JObject order,
        CancellationToken ct)
    {
        var address =
            order["shipping_address"]?["address1"]?
                .ToString()?.ToLowerInvariant() ?? "";

        var phone =
            order["shipping_address"]?["phone"]?.ToString();

        if (string.IsNullOrWhiteSpace(phone))
        {
            return Task.FromResult<OrderTagResult?>(new OrderTagResult
            {
                Tag = "ara1",
                Priority = 100,
                ReasonCode = "MISSING_PHONE",
                Notes = { "Telefon numarası eksik" }
            });
        }

        if (address.Length < 10 ||
            WeakKeywords.Any(k => address.Contains(k)))
        {
            return Task.FromResult<OrderTagResult?>(new OrderTagResult
            {
                Tag = "ara1",
                Priority = 95,
                ReasonCode = "ADDRESS_INSUFFICIENT",
                Notes =
                {
                    "Adres yetersiz veya teslim noktası içeriyor"
                }
            });
        }

        return Task.FromResult<OrderTagResult?>(new OrderTagResult
        {
            Tag = "dhl",
            Priority = 10,
            Notes = { "Standart şehir içi teslimat" }
        });
    }
}
