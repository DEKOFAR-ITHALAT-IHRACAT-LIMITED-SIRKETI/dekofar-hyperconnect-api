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
        // 👤 2. AYNI MÜŞTERİ – MUTLAK ARA1
        // =====================================================

        // Aynı anda birden fazla açık sipariş
        var repeatPhoneCount =
            order["__repeat_phone_count"]?.Value<int>() ?? 0;

        // Daha önce / başka siparişi var
        var customerOrders =
            order["customer"]?["orders_count"]?.Value<int>() ?? 0;

        if (repeatPhoneCount > 1 || customerOrders > 1)
        {
            result.Decision = OrderDecision.ApprovalNeeded;
            result.Reasons.Add(
                "Müşterinin birden fazla siparişi bulunduğu için tüm siparişler onaya alındı");
            return result; // 🔥 DHL ihtimali sıfır
        }

        // =====================================================
        // 💰 3. TUTAR
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
            result.Reasons.Add("Birden fazla ürün bulunuyor");
        }

        // =====================================================
        // 📍 5. ADRES
        // =====================================================
        var addressResult = AddressValidator.Validate(order);

        if (!addressResult.IsValid)
        {
            result.Decision = OrderDecision.ApprovalNeeded;
            result.Reasons.AddRange(addressResult.Reasons);
        }

        // =====================================================
        // 🟢 6. DHL
        // =====================================================
        if (result.Decision == default)
        {
            result.Decision = OrderDecision.Automatic;
        }

        return result;
    }

    private static bool ContainsCancelKeyword(JObject order)
    {
        var note = order["note"]?.ToString() ?? string.Empty;
        var address =
            order["shipping_address"]?["address1"]?.ToString() ?? string.Empty;

        var text = $"{note} {address}".ToLowerInvariant();
        return CancelKeywords.Any(k => text.Contains(k));
    }
}
