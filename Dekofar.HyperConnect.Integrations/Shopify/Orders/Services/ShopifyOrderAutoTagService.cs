using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
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
    /// ✔ Stateless
    /// </summary>
    public class ShopifyOrderAutoTagService
    {
        private readonly ShopifyGraphQlClient _graphQl;
        private readonly OrderDecisionEngine _decisionEngine;
        private readonly IApplicationDbContext _db;

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
        // 🚀 ENTRY POINT
        // =====================================================
        public async Task ApplyAutoTagsAsync(
            JObject order,
            string shopDomain,
            CancellationToken ct,
            bool replaceExistingTags = true)
        {
            if (string.IsNullOrWhiteSpace(shopDomain))
                throw new InvalidOperationException("shopDomain is required");

            var orderId =
                order["admin_graphql_api_id"]?.ToString();

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
            // 🧠 DECISION
            // =====================================================
            var decision = _decisionEngine.Decide(order);

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
            // 🧹 TAG TEMİZLE
            // =====================================================
            if (replaceExistingTags)
            {
                var existingTags =
                    order["tags"]?.ToString();

                var tagsToRemove = existingTags?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .ToArray();

                if (tagsToRemove is { Length: > 0 })
                {
                    await _graphQl.ExecuteAsync(
                        store.ShopDomain,
                        store.AccessToken,
                        GraphQlMutations.TagsRemove,
                        new { id = orderId, tags = tagsToRemove },
                        ct);
                }
            }

            // =====================================================
            // 🏷️ TAG EKLE
            // =====================================================
            await _graphQl.ExecuteAsync(
                store.ShopDomain,
                store.AccessToken,
                GraphQlMutations.TagsAdd,
                new { id = orderId, tags = new[] { tag } },
                ct);

            // =====================================================
            // 📝 SİSTEM NOTU (ara1)
            // =====================================================
            if (decision.Decision == OrderDecision.ApprovalNeeded &&
                decision.Reasons.Any())
            {
                var systemNote =
                    "[SİSTEM]\n" +
                    string.Join("\n",
                        decision.Reasons
                            .Distinct()
                            .Select(r => $"• {r}"));

                var existingNote =
                    order["note"]?.ToString();

                var finalNote = string.IsNullOrWhiteSpace(existingNote)
                    ? systemNote
                    : $"{systemNote}\n\n[MÜŞTERİ NOTU]\n{existingNote}";

                await _graphQl.ExecuteAsync(
                    store.ShopDomain,
                    store.AccessToken,
                    GraphQlMutations.UpdateOrderNote,
                    new { id = orderId, note = finalNote },
                    ct);
            }

            // =====================================================
            // 🔥 KRİTİK KURAL
            // AYNI TELEFON → TÜM AÇIKLAR ara1
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
        // 🔥 AYNI TELEFON → TÜM AÇIK SİPARİŞLER ara1
        // =====================================================
        private async Task ForceAllOpenOrdersWithSamePhoneToAra1Async(
            ShopifyStore store,
            JObject order,
            CancellationToken ct)
        {
            var phone =
                order["shipping_address"]?["phone"]?.ToString();

            if (string.IsNullOrWhiteSpace(phone))
                return;

            var json = await _graphQl.ExecuteAsync(
                store.ShopDomain,
                store.AccessToken,
                GraphQlQueries.OrdersByPhone,
                new { phone },
                ct);

            if (json?["data"]?["orders"]?["edges"] is not JArray edges)
                return;

            foreach (var edge in edges)
            {
                if (edge?["node"] is not JObject node)
                    continue;

                var orderId = node["id"]?.ToString();
                if (string.IsNullOrWhiteSpace(orderId))
                    continue;

                var existingTags =
                    node["tags"]?.ToString() ?? string.Empty;

                var tagsToRemove = existingTags
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => t != "ara1")
                    .ToArray();

                if (tagsToRemove.Length > 0)
                {
                    await _graphQl.ExecuteAsync(
                        store.ShopDomain,
                        store.AccessToken,
                        GraphQlMutations.TagsRemove,
                        new { id = orderId, tags = tagsToRemove },
                        ct);
                }

                if (!existingTags.Contains("ara1"))
                {
                    await _graphQl.ExecuteAsync(
                        store.ShopDomain,
                        store.AccessToken,
                        GraphQlMutations.TagsAdd,
                        new { id = orderId, tags = new[] { "ara1" } },
                        ct);
                }
            }
        }
    }

    // =====================================================
    // 🧠 GRAPHQL SABİTLERİ
    // =====================================================
    internal static class GraphQlMutations
    {
        public const string TagsAdd = @"
mutation ($id: ID!, $tags: [String!]!) {
  tagsAdd(id: $id, tags: $tags) {
    userErrors { message }
  }
}";

        public const string TagsRemove = @"
mutation ($id: ID!, $tags: [String!]!) {
  tagsRemove(id: $id, tags: $tags) {
    userErrors { message }
  }
}";

        public const string UpdateOrderNote = @"
mutation ($id: ID!, $note: String!) {
  orderUpdate(input: { id: $id, note: $note }) {
    userErrors { message }
  }
}";
    }

    internal static class GraphQlQueries
    {
        public const string OrdersByPhone = @"
query ($phone: String!) {
  orders(
    first: 50
    query: ""financial_status:pending fulfillment_status:unfulfilled phone:$phone""
  ) {
    edges {
      node {
        id
        tags
      }
    }
  }
}";
    }
}
