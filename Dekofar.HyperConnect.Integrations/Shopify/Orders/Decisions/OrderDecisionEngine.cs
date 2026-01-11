using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Decisions
{
    public sealed class OrderDecisionEngine
    {
        private static readonly string[] CancelKeywords =
        {
            "iptal",
            "test",
            "deneme",
            "sahte",
            "fake"
        };

        private static readonly string[] ApprovalNoteKeywords =
        {
            "şube",
            "şubeye",
            "kargo gönderme",
            "kargo göndermeyin",
            "göndermeyin",
            "elden teslim"
        };

        public OrderDecisionResult Decide(JObject order)
        {
            var reasons = new List<string>();
            var isForcedApproval = false;

            // =====================================================
            // 🔴 1. MUTLAK IPTAL
            // =====================================================
            if (ContainsCancelKeyword(order))
            {
                return new OrderDecisionResult(
                    decision: OrderDecision.Cancelled,
                    reasons: new[]
                    {
                        "Siparişte iptal / test amaçlı ifade tespit edildi"
                    },
                    isForcedApproval: false
                );
            }

            // =====================================================
            // 👤 2. AYNI TELEFON / TEKRAR SİPARİŞ
            // (Kapalı siparişler SAYILIR → kararı etkiler)
            // =====================================================
            var repeatPhoneCount =
                order["__repeat_phone_count"]?.Value<int>() ?? 0;

            var customerOrders =
                order["customer"]?["orders_count"]?.Value<int>() ?? 0;

            if (repeatPhoneCount > 1 || customerOrders > 1)
            {
                reasons.Add("Aynı telefon numarasıyla birden fazla sipariş mevcut");
                isForcedApproval = true;
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
                reasons.Add("Sipariş tutarı 1000 TL altında");
            }
            else if (total >= 2000)
            {
                reasons.Add("Sipariş tutarı 2000 TL ve üzeri");
            }

            // =====================================================
            // 📦 4. AYNI ÜRÜNDEN FAZLA ADET
            // =====================================================
            var hasMultipleQuantity =
                order["line_items"]?.Any(li =>
                    li["quantity"]?.Value<int>() > 1) == true;

            if (hasMultipleQuantity)
            {
                reasons.Add("Aynı üründen birden fazla adet sipariş edilmiş");
            }

            // =====================================================
            // 📍 5. ADRES KONTROLÜ (20 KARAKTER)
            // =====================================================
            var address =
                order["shipping_address"]?["address1"]?.ToString();

            if (string.IsNullOrWhiteSpace(address) || address.Length < 20)
            {
                reasons.Add("Adres 20 karakterden kısa veya eksik");
            }

            // =====================================================
            // 📝 6. MÜŞTERİ NOTU KONTROLÜ
            // =====================================================
            var note =
                order["note"]?.ToString()?.ToLowerInvariant() ?? string.Empty;

            if (ApprovalNoteKeywords.Any(k => note.Contains(k)))
            {
                reasons.Add("Müşteri notunda kargo / teslimat kısıtı var");
            }

            // =====================================================
            // 🧠 FINAL KARAR
            // =====================================================
            if (reasons.Count > 0)
            {
                return new OrderDecisionResult(
                    decision: OrderDecision.ApprovalNeeded,
                    reasons: reasons,
                    isForcedApproval: isForcedApproval
                );
            }

            return new OrderDecisionResult(
                decision: OrderDecision.Automatic,
                reasons: Array.Empty<string>(),
                isForcedApproval: false
            );
        }

        // =====================================================
        // 🔍 IPTAL ANAHTAR KELİME
        // =====================================================
        private static bool ContainsCancelKeyword(JObject order)
        {
            var note =
                order["note"]?.ToString() ?? string.Empty;

            var address =
                order["shipping_address"]?["address1"]?.ToString() ?? string.Empty;

            var text =
                $"{note} {address}".ToLowerInvariant();

            return CancelKeywords.Any(k => text.Contains(k));
        }
    }
}
