using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Decisions;

public class OrderDecisionEngine
{
    private static readonly string[] CancelKeywords =
    {
        "iptal",
        "test",
        "deneme",
        "sahte",
        "fake"
    };

    public OrderDecisionResult Decide(JObject order)
    {
        var result = new OrderDecisionResult();

        // =====================================================
        // 🔴 1. MUTLAK IPTAL
        // =====================================================
        if (ContainsCancelKeyword(order))
        {
            result.Decision = OrderDecision.Cancelled;
            result.Reasons.Add("Siparişte iptal/test amaçlı ifade tespit edildi");
            return result;
        }

        // =====================================================
        // 👤 2. AYNI MÜŞTERİDEN BİRDEN FAZLA AÇIK SİPARİŞ (MUTLAK ARA1)
        // =====================================================
        var repeatPhoneCount =
            order["__repeat_phone_count"]?.Value<int>() ?? 0;

        if (repeatPhoneCount > 1)
        {
            result.Decision = OrderDecision.ApprovalNeeded;
            result.Reasons.Add(
                "Müşterinin birden fazla açık siparişi bulunduğu için tüm siparişler onaya alındı");

            // 🔥 BURADA DÖNÜYORUZ → DHL OLMASI İMKANSIZ
            return result;
        }

        // =====================================================
        // 💰 3. TUTAR KONTROLÜ
        // =====================================================
        decimal.TryParse(
            order["total_price"]?.ToString(),
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out var total);

        if (total < 1000)
        {
            result.Decision = OrderDecision.ApprovalNeeded;
            result.Reasons.Add("Sipariş tutarı 1000 TL altında (kargo ücreti)");
        }
        else if (total >= 2000)
        {
            result.Decision = OrderDecision.ApprovalNeeded;
            result.Reasons.Add("Sipariş tutarı 2000 TL ve üzeri (yüksek tutar)");
        }

        // =====================================================
        // 📦 4. ÜRÜN SAYISI
        // =====================================================
        var distinctProducts =
            order["line_items"]?
                .Select(li => li["product_id"]?.ToString())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .Count() ?? 0;

        if (distinctProducts > 1)
        {
            result.Decision = OrderDecision.ApprovalNeeded;
            result.Reasons.Add("Siparişte birden fazla ürün çeşidi bulunuyor");
        }

        // =====================================================
        // 📍 5. ADRES DOĞRULAMA
        // =====================================================
        var addressResult = AddressValidator.Validate(order);

        if (!addressResult.IsValid)
        {
            result.Decision = OrderDecision.ApprovalNeeded;
            result.Reasons.AddRange(addressResult.Reasons);
        }

        // =====================================================
        // 🟢 6. OTOMATİK (DHL)
        // =====================================================
        if (result.Decision == default)
        {
            result.Decision = OrderDecision.Automatic;
        }

        return result;
    }

    // =====================================================
    // 🔍 IPTAL KELİME KONTROLÜ
    // =====================================================
    private static bool ContainsCancelKeyword(JObject order)
    {
        var note = order["note"]?.ToString() ?? string.Empty;
        var address =
            order["shipping_address"]?["address1"]?.ToString() ?? string.Empty;

        var text =
            $"{note} {address}".ToLowerInvariant();

        return CancelKeywords.Any(k => text.Contains(k));
    }
}
