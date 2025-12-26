using Newtonsoft.Json.Linq;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class ShippingDecisionRule : IOrderTagRule
{
    private static readonly string[] VillageKeywords =
    {
        "köy", "köyü", "mezra"
    };

    private static readonly string[] WeakAddressKeywords =
    {
        "avm",
        "sinema",
        "kargo",
        "kargodan",
        "şube",
        "teslim al",
        "hastane"
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

        // 🔴 Telefon yok → ARA1
        if (string.IsNullOrWhiteSpace(phone))
        {
            return Task.FromResult<OrderTagResult?>(
                Ara1("Telefon numarası eksik"));
        }

        // 🔴 Çok kısa adres → ARA1
        if (address.Length < 10)
        {
            return Task.FromResult<OrderTagResult?>(
                Ara1("Adres çok kısa"));
        }

        // 🔴 AVM / hastane / şube vb.
        if (WeakAddressKeywords.Any(k => address.Contains(k)))
        {
            return Task.FromResult<OrderTagResult?>(
                Ara1("Teslimat için yetersiz adres"));
        }

        // 🟡 Köy → PTT
        if (VillageKeywords.Any(k => address.Contains(k)))
        {
            return Task.FromResult<OrderTagResult?>(
                new OrderTagResult
                {
                    Tag = "ptt",
                    Reason = "Adres köy/mezra içeriyor"
                });
        }

        // 🟢 Varsayılan → DHL
        return Task.FromResult<OrderTagResult?>(
            new OrderTagResult
            {
                Tag = "dhl",
                Reason = "Şehir içi temiz adres"
            });
    }

    private static OrderTagResult Ara1(string reason) =>
        new()
        {
            Tag = "ara1",
            Reason = reason
        };
}
