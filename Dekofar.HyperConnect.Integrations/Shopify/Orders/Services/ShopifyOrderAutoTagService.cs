using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.Constants;
using Dekofar.HyperConnect.Integrations.Shopify.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Decisions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services
{
    /// <summary>
    /// Shopify Order Auto Tag Service
    /// ✔ Webhook + manuel reprocess uyumlu
    /// ✔ Reset flag destekli
    /// ✔ Eski sistem notlarını temizler
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

        public async Task ApplyAutoTagsAsync(
            JObject order,
            string shopDomain,
            CancellationToken ct,
            bool replaceExistingTags = true,
            bool ignoreResetFlag = false)
        {
            var orderId = order["admin_graphql_api_id"]?.ToString();
            if (string.IsNullOrWhiteSpace(orderId))
                return;

            var store = await _db.ShopifyStores
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ShopDomain == shopDomain && x.IsActive, ct);

            if (store == null)
                return;

            // 🔒 Reset flag (sadece webhook için)
            if (!ignoreResetFlag)
            {
                var note = order["note"]?.ToString();
                if (!string.IsNullOrWhiteSpace(note) &&
                    note.Contains(ShopifySystemNotes.ResetFlag))
                    return;
            }

            var decision = _decisionEngine.Decide(order);

            if (IsAddressTooShort(order) || HasBlockedCustomerNote(order))
                decision.Decision = OrderDecision.ApprovalNeeded;

            var tag = decision.Decision switch
            {
                OrderDecision.Automatic => "dhl",
                OrderDecision.ApprovalNeeded => "ara1",
                OrderDecision.Cancelled => "iptal",
                _ => null
            };

            if (tag == null)
                return;

            // 🧹 Eski tag'leri sil
            if (replaceExistingTags)
            {
                var tagsRaw = order["tags"]?.ToString();
                var tagsToRemove = tagsRaw?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .ToArray();

                if (tagsToRemove?.Length > 0)
                {
                    await _graphQl.ExecuteAsync(
                        store.ShopDomain,
                        store.AccessToken,
                        ShopifyGraphQlMutations.TagsRemove,
                        new { id = orderId, tags = tagsToRemove },
                        ct);
                }
            }

            // 🏷 Yeni tag ekle
            await _graphQl.ExecuteAsync(
                store.ShopDomain,
                store.AccessToken,
                ShopifyGraphQlMutations.TagsAdd,
                new { id = orderId, tags = new[] { tag } },
                ct);

            // 📝 Sistem notu
            if (decision.Decision == OrderDecision.ApprovalNeeded)
            {
                var cleanNote = RemoveSystemNotes(order["note"]?.ToString());

                var systemNote =
                    ShopifySystemNotes.SystemNotePrefix + "\n" +
                    string.Join("\n", decision.Reasons.Distinct().Select(r => $"• {r}"));

                var finalNote = string.IsNullOrWhiteSpace(cleanNote)
                    ? systemNote
                    : $"{systemNote}\n\n[MÜŞTERİ NOTU]\n{cleanNote}";

                await _graphQl.ExecuteAsync(
                    store.ShopDomain,
                    store.AccessToken,
                    ShopifyGraphQlMutations.UpdateOrderNote,
                    new { id = orderId, note = finalNote },
                    ct);
            }
        }

        private static bool IsAddressTooShort(JObject order)
        {
            var address = order["shipping_address"]?["address1"]?.ToString();
            return string.IsNullOrWhiteSpace(address) || address.Length < 20;
        }

        private static bool HasBlockedCustomerNote(JObject order)
        {
            var note = order["note"]?.ToString()?.ToLowerInvariant();
            return !string.IsNullOrWhiteSpace(note) &&
                   NoteBlockKeywords.Any(k => note.Contains(k));
        }

        private static string? RemoveSystemNotes(string? note)
        {
            if (string.IsNullOrWhiteSpace(note))
                return note;

            return note
                .Replace(ShopifySystemNotes.ResetFlag, string.Empty)
                .Split(ShopifySystemNotes.SystemNotePrefix)
                .FirstOrDefault()
                ?.Trim();
        }
    }
}
