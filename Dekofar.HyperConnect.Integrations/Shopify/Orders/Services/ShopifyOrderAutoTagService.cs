using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Decisions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services
{
    /// <summary>
    /// Shopify Order Auto Tag Service
    /// ✔ OAuth token DB’den
    /// ✔ Multi-store safe
    /// ✔ Webhook / manuel reprocess uyumlu
    /// ✔ Aynı telefon → sadece AÇIK siparişler ara1
    /// </summary>
    public sealed class ShopifyOrderAutoTagService
    {
        private readonly ShopifyGraphQlClient _graphQl;
        private readonly OrderDecisionEngine _decisionEngine;
        private readonly IApplicationDbContext _db;

        private static readonly string[] NoteBlockKeywords =
        {
            "şube",
            "şubeden",
            "kargo göndermeyin",
            "elden",
            "aramayın"
        };

        public ShopifyOrderAutoTagService(
            ShopifyGraphQlClient graphQl,
            OrderDecisionEngine decisionEngine,
            IApplicationDbContext db)
        {
            _graphQl = graphQl;
            _decisionEngine = decisionEngine;
            _db = db;
        }

        // =====================================================
        // 🚀 ENTRY
        // =====================================================
        public async Task ApplyAutoTagsAsync(
            JObject order,
            string shopDomain,
            CancellationToken ct,
            bool replaceExistingTags = true)
        {
            if (string.IsNullOrWhiteSpace(shopDomain))
                throw new InvalidOperationException("shopDomain is required");

            var orderId = order["admin_graphql_api_id"]?.ToString();
            if (string.IsNullOrWhiteSpace(orderId))
                return;

            // =====================================================
            // 🔑 STORE
            // =====================================================
            var store = await _db.ShopifyStores
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.ShopDomain == shopDomain && x.IsActive,
                    ct);

            if (store == null)
                throw new InvalidOperationException(
                    $"ShopifyStore not found or inactive: {shopDomain}");

            // =====================================================
            // 🧠 BASE DECISION (ENGINE)
            // =====================================================
            var decision = _decisionEngine.Decide(order);

            // =====================================================
            // ➕ EK ARA1 KURALLARI
            // =====================================================
            if (IsAddressTooShort(order))
            {
                decision.Decision = OrderDecision.ApprovalNeeded;
                decision.Reasons.Add("Adres 20 karakterden kısa veya eksik");
            }

            if (HasBlockedCustomerNote(order))
            {
                decision.Decision = OrderDecision.ApprovalNeeded;
                decision.Reasons.Add("Müşteri notunda teslimat / kargo kısıtı var");
            }

            var tag = decision.Decision switch
            {
                OrderDecision.Automatic => "dhl",
                OrderDecision.ApprovalNeeded => "ara1",
                OrderDecision.Cancelled => "iptal",
                _ => null
            };

            if (string.IsNullOrWhiteSpace(tag))
                return;

            // =====================================================
            // 🧹 MEVCUT TAG’LERİ TEMİZLE
            // =====================================================
            if (replaceExistingTags)
            {
                var existingTags = order["tags"]?.ToString();

                var tagsToRemove = existingTags?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .ToArray();

                if (tagsToRemove is { Length: > 0 })
                {
                    await _graphQl.ExecuteAsync(
                        store.ShopDomain,
                        store.AccessToken,
                        ShopifyGraphQlMutations.TagsRemove,
                        new { id = orderId, tags = tagsToRemove },
                        ct);
                }
            }

            // =====================================================
            // 🏷️ YENİ TAG EKLE
            // =====================================================
            await _graphQl.ExecuteAsync(
                store.ShopDomain,
                store.AccessToken,
                ShopifyGraphQlMutations.TagsAdd,
                new { id = orderId, tags = new[] { tag } },
                ct);

            // =====================================================
            // 📝 SİSTEM NOTU (SADECE ara1)
            // =====================================================
            if (decision.Decision == OrderDecision.ApprovalNeeded &&
                decision.Reasons.Any())
            {
                var systemNote =
                    "[SİSTEM]\n" +
                    string.Join(
                        "\n",
                        decision.Reasons
                            .Distinct()
                            .Select(r => $"• {r}")
                    );

                var existingNote = order["note"]?.ToString();

                var finalNote = string.IsNullOrWhiteSpace(existingNote)
                    ? systemNote
                    : $"{systemNote}\n\n[MÜŞTERİ NOTU]\n{existingNote}";

                await _graphQl.ExecuteAsync(
                    store.ShopDomain,
                    store.AccessToken,
                    ShopifyGraphQlMutations.UpdateOrderNote,
                    new { id = orderId, note = finalNote },
                    ct);
            }

            // =====================================================
            // 📞 AYNI TELEFON → SADECE AÇIK SİPARİŞLER ara1
            // =====================================================
            if (decision.Decision == OrderDecision.ApprovalNeeded)
            {
                await ForceAllOpenOrdersWithSamePhoneToAra1Async(
                    store,
                    order,
                    ct);
            }
        }

        // =====================================================
        // 📞 AYNI TELEFON → TÜM AÇIK SİPARİŞLERİ ara1
        // =====================================================
        private async Task ForceAllOpenOrdersWithSamePhoneToAra1Async(
            ShopifyStore store,
            JObject order,
            CancellationToken ct)
        {
            var phone = order["shipping_address"]?["phone"]?.ToString();
            if (string.IsNullOrWhiteSpace(phone))
                return;

            var json = await _graphQl.ExecuteAsync(
                store.ShopDomain,
                store.AccessToken,
                ShopifyGraphQlQueries.OpenOrdersByPhone,
                new { phone },
                ct);

            if (json?["data"]?["orders"]?["edges"] is not JArray edges)
                return;

            // Tek açık sipariş varsa zorlamaya gerek yok
            if (edges.Count <= 1)
                return;

            foreach (var edge in edges)
            {
                if (edge?["node"] is not JObject node)
                    continue;

                var orderId = node["id"]?.ToString();
                if (string.IsNullOrWhiteSpace(orderId))
                    continue;

                var existingTags = node["tags"]?.ToString() ?? string.Empty;
                if (existingTags.Contains("ara1"))
                    continue;

                var tagsToRemove = existingTags
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .ToArray();

                if (tagsToRemove.Length > 0)
                {
                    await _graphQl.ExecuteAsync(
                        store.ShopDomain,
                        store.AccessToken,
                        ShopifyGraphQlMutations.TagsRemove,
                        new { id = orderId, tags = tagsToRemove },
                        ct);
                }

                await _graphQl.ExecuteAsync(
                    store.ShopDomain,
                    store.AccessToken,
                    ShopifyGraphQlMutations.TagsAdd,
                    new { id = orderId, tags = new[] { "ara1" } },
                    ct);
            }
        }

        // =====================================================
        // 🔍 HELPERS
        // =====================================================
        private static bool IsAddressTooShort(JObject order)
        {
            var address =
                order["shipping_address"]?["address1"]?.ToString();

            return string.IsNullOrWhiteSpace(address) || address.Length < 20;
        }

        private static bool HasBlockedCustomerNote(JObject order)
        {
            var note = order["note"]?.ToString()?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(note))
                return false;

            return NoteBlockKeywords.Any(k => note.Contains(k));
        }
    }
}
