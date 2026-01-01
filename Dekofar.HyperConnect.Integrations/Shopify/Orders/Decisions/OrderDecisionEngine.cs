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
        // 🔴 1. MUTLAK IPTAL (HER ŞEYİN ÜSTÜNDE)
        // =====================================================
        if (ContainsCancelKeyword(order))
        {
            result.Decision = OrderDecision.Cancelled;
            result.Reasons.Add("Siparişte iptal/test amaçlı ifade tespit edildi");
            return result;
        }

        // =====================================================
        // 🔴 2. AYNI MÜŞTERİ / TELEFON → MUTLAK ONAY
        // (BİRİ ONAY OLSA BİLE, DİĞERİ VARSA HEPSİ ARA)
        // =====================================================
        var repeatPhoneCount =
            order["__repeat_phone_count"]?.Value<int>() ?? 0;

        var customerOrders =
            order["customer"]?["orders_count"]?.Value<int>() ?? 0;

        if (repeatPhoneCount > 1 || customerOrders > 1)
        {
            result.Decision = OrderDecision.ApprovalNeeded;
            result.Reasons.Add(
                "Müşterinin birden fazla açık siparişi bulunduğu için tüm siparişler onaya alındı");

            // 🔥 ALT KURALLAR ÇALIŞMAZ
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
        // 🟢 6. SON KARAR
        // (BURAYA SADECE TEMİZ, TEK SİPARİŞ DÜŞER)
        // =====================================================
        if (result.Decision == default)
        {
            // 1000–2000 TL + tüm şartlar OK
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
