using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class ShippingDecisionRule : IOrderTagRule
{
    private static readonly string[] VillageKeywords =
    {
        "köy", "köyü", "mezra"
    };

    private static readonly string[] WeakAddressKeywords =
    {
        "avm", "sinema", "kargo", "kargodan",
        "şube", "teslim al", "hastane"
    };

    public Task<OrderTagResult?> EvaluateAsync(JObject order, CancellationToken ct)
    {
        var address =
            order["shipping_address"]?["address1"]?
                .ToString()?.ToLowerInvariant() ?? "";

        var phone =
            order["shipping_address"]?["phone"]?.ToString();

        if (string.IsNullOrWhiteSpace(phone))
            return Task.FromResult<OrderTagResult?>(Ara1("Telefon numarası eksik"));

        if (address.Length < 10)
            return Task.FromResult<OrderTagResult?>(Ara1("Adres çok kısa"));

        if (WeakAddressKeywords.Any(k => address.Contains(k)))
            return Task.FromResult<OrderTagResult?>(Ara1("Teslimat için yetersiz adres"));

        if (VillageKeywords.Any(k => address.Contains(k)))
        {
            var r = new OrderTagResult
            {
                Tag = "ptt",
                Priority = 50
            };
            r.Notes.Add("Adres köy / mezra içeriyor");
            return Task.FromResult<OrderTagResult?>(r);
        }

        var dhl = new OrderTagResult
        {
            Tag = "dhl",
            Priority = 10
        };
        dhl.Notes.Add("Şehir içi temiz adres");
        return Task.FromResult<OrderTagResult?>(dhl);
    }

    private static OrderTagResult Ara1(string reason)
    {
        var r = new OrderTagResult
        {
            Tag = "ara1",
            Priority = 100
        };
        r.Reasons.Add(reason);
        r.Notes.Add(reason);
        return r;
    }
}
