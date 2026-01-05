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
            result.Reasons.Add("Siparişte iptal / test amaçlı ifade tespit edildi");
            return result;
        }

        // =====================================================
        // 👤 2. AYNI MÜŞTERİ → MUTLAK ara1
        // =====================================================
        var repeatPhoneCount =
            order["__repeat_phone_count"]?.Value<int>() ?? 0;

        var customerOrders =
            order["customer"]?["orders_count"]?.Value<int>() ?? 0;

        if (repeatPhoneCount > 1 || customerOrders > 1)
        {
            result.Decision = OrderDecision.ApprovalNeeded;
            result.Reasons.Add(
                "Müşterinin birden fazla siparişi bulunduğu için tüm siparişler ara1'e alındı");
            return result; // ❗ burada dhl ihtimali sıfır
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
            result.Reasons.Add("Sipariş tutarı 1000 TL altında");
        }
        else if (total >= 2000)
        {
            result.Decision = OrderDecision.ApprovalNeeded;
            result.Reasons.Add("Sipariş tutarı 2000 TL ve üzeri");
        }

        // =====================================================
        // 📦 4. AYNI ÜRÜNDEN BİRDEN FAZLA ADET (quantity > 1)
        // =====================================================
        var hasMultipleQuantity =
            order["line_items"]?
                .Any(li => li["quantity"]?.Value<int>() > 1)
            == true;

        if (hasMultipleQuantity)
        {
            result.Decision = OrderDecision.ApprovalNeeded;
            result.Reasons.Add("Aynı üründen birden fazla adet sipariş edilmiş");
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
        // 🟢 6. DHL (SADECE HİÇBİR ŞARTA TAKILMIYORSA)
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

        var text = $"{note} {address}".ToLowerInvariant();

        return CancelKeywords.Any(k => text.Contains(k));
    }
}
