using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services
{
    /// <summary>
    /// Swagger üzerinden çalıştırılan
    /// AÇIK sipariş RESET + yeniden etiketleme servisi
    /// </summary>
    public sealed class ShopifyOrderReprocessService
    {
        private readonly ShopifyGraphQlClient _graphQl;
        private readonly ShopifyOrderAutoTagService _autoTag;
        private readonly IApplicationDbContext _db;

        private const int BatchSize = 100;
        private static readonly TimeSpan BatchDelay = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan ConsistencyDelay = TimeSpan.FromSeconds(5);

        public ShopifyOrderReprocessService(
            ShopifyGraphQlClient graphQl,
            ShopifyOrderAutoTagService autoTag,
            IApplicationDbContext db)
        {
            _graphQl = graphQl;
            _autoTag = autoTag;
            _db = db;
        }

        // =====================================================
        // 🚀 ENTRY (Swagger burayı çağırır)
        // =====================================================
        public async Task<int> ReprocessOpenOrdersAsync(
            string shopDomain,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(shopDomain))
                throw new InvalidOperationException("shopDomain is required");

            var store = await GetStoreAsync(shopDomain, ct);

            // 1️⃣ TÜM AÇIK SİPARİŞLERDE ETİKETLERİ SİL
            await ClearAllTagsForOpenOrdersAsync(store, ct);

            // Shopify eventual consistency
            await Task.Delay(ConsistencyDelay, ct);

            // 2️⃣ RESET FLAG YOK SAYILARAK YENİDEN ETİKETLE
            return await ReprocessOpenOrdersInBatchesAsync(store, ct);
        }

        // =====================================================
        // 🧹 AŞAMA 1 — TÜM ETİKETLERİ TEMİZLE
        // =====================================================
        private async Task ClearAllTagsForOpenOrdersAsync(
            ShopifyStore store,
            CancellationToken ct)
        {
            string? cursor = null;
            bool hasNextPage;

            do
            {
                ct.ThrowIfCancellationRequested();

                var json = await _graphQl.ExecuteAsync(
                    store.ShopDomain,
                    store.AccessToken,
                    GraphQlQueries.OpenOrdersMinimal,
                    new { cursor, first = BatchSize },
                    ct);

                var orders = json["data"]?["orders"] as JObject;
                if (orders == null)
                    break;

                hasNextPage =
                    orders["pageInfo"]?["hasNextPage"]?.Value<bool>() == true;

                cursor =
                    orders["pageInfo"]?["endCursor"]?.ToString();

                if (orders["edges"] is not JArray edges)
                    continue;

                foreach (var edge in edges)
                {
                    if (edge["node"] is not JObject node)
                        continue;

                    var orderId = node["id"]?.ToString();
                    var tagsRaw = node["tags"]?.ToString();

                    if (string.IsNullOrWhiteSpace(orderId) ||
                        string.IsNullOrWhiteSpace(tagsRaw))
                        continue;

                    var tags = tagsRaw
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                        .ToArray();

                    if (tags.Length == 0)
                        continue;

                    await _graphQl.ExecuteAsync(
                        store.ShopDomain,
                        store.AccessToken,
                        GraphQlMutations.TagsRemove,
                        new { id = orderId, tags },
                        ct);
                }

                if (hasNextPage)
                    await Task.Delay(BatchDelay, ct);

            } while (hasNextPage);
        }

        // =====================================================
        // 🔁 AŞAMA 2 — YENİDEN ETİKETLE
        // =====================================================
        private async Task<int> ReprocessOpenOrdersInBatchesAsync(
            ShopifyStore store,
            CancellationToken ct)
        {
            string? cursor = null;
            bool hasNextPage;
            int processed = 0;

            do
            {
                ct.ThrowIfCancellationRequested();

                var json = await _graphQl.ExecuteAsync(
                    store.ShopDomain,
                    store.AccessToken,
                    GraphQlQueries.OpenOrdersFull,
                    new { cursor, first = BatchSize },
                    ct);

                var orders = json["data"]?["orders"] as JObject;
                if (orders == null)
                    break;

                hasNextPage =
                    orders["pageInfo"]?["hasNextPage"]?.Value<bool>() == true;

                cursor =
                    orders["pageInfo"]?["endCursor"]?.ToString();

                if (orders["edges"] is not JArray edges || edges.Count == 0)
                    continue;

                // 📞 batch içi telefon tekrar sayacı
                var phoneCounts = new Dictionary<string, int>();

                foreach (var edge in edges)
                {
                    var phone =
                        edge["node"]?["shippingAddress"]?["phone"]?.ToString();

                    if (string.IsNullOrWhiteSpace(phone))
                        continue;

                    phoneCounts.TryGetValue(phone, out var c);
                    phoneCounts[phone] = c + 1;
                }

                foreach (var edge in edges)
                {
                    if (edge["node"] is not JObject node)
                        continue;

                    try
                    {
                        var normalized =
                            NormalizeGraphQlOrder(node, phoneCounts);

                        // 🔥 RESET FLAG YOK SAYILIR
                        await _autoTag.ApplyAutoTagsAsync(
                            normalized,
                            store.ShopDomain,
                            ct,
                            replaceExistingTags: false,
                            ignoreResetFlag: true);

                        processed++;
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (hasNextPage)
                    await Task.Delay(BatchDelay, ct);

            } while (hasNextPage);

            return processed;
        }

        // =====================================================
        // 🔄 NORMALIZE
        // =====================================================
        private static JObject NormalizeGraphQlOrder(
            JObject node,
            Dictionary<string, int> phoneCounts)
        {
            var shipping = node["shippingAddress"] as JObject;
            var customer = node["customer"] as JObject;

            var phone = shipping?["phone"]?.ToString();
            phoneCounts.TryGetValue(phone ?? string.Empty, out var repeat);

            return new JObject
            {
                ["admin_graphql_api_id"] = node["id"]?.ToString(),
                ["tags"] = "",
                ["note"] = node["note"]?.ToString(),
                ["shipping_address"] = new JObject
                {
                    ["address1"] = shipping?["address1"]?.ToString(),
                    ["phone"] = phone
                },
                ["customer"] = new JObject
                {
                    ["orders_count"] =
                        customer?["numberOfOrders"]?.Value<int>() ?? 0
                },
                ["__repeat_phone_count"] = repeat
            };
        }

        // =====================================================
        // 🔑 STORE
        // =====================================================
        private async Task<ShopifyStore> GetStoreAsync(
            string shopDomain,
            CancellationToken ct)
        {
            var store = await _db.ShopifyStores
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.ShopDomain == shopDomain && x.IsActive,
                    ct);

            if (store == null)
                throw new InvalidOperationException(
                    $"ShopifyStore not found or inactive: {shopDomain}");

            return store;
        }
    }

    // =====================================================
    // 🧠 GRAPHQL
    // =====================================================
    internal static class GraphQlQueries
    {
        public const string OpenOrdersMinimal = @"
query ($cursor: String, $first: Int!) {
  orders(
    first: $first
    after: $cursor
    query: ""financial_status:pending fulfillment_status:unfulfilled""
  ) {
    pageInfo { hasNextPage endCursor }
    edges {
      node {
        id
        tags
      }
    }
  }
}";
        public const string OpenOrdersFull = @"
query ($cursor: String, $first: Int!) {
  orders(
    first: $first
    after: $cursor
    query: ""financial_status:pending fulfillment_status:unfulfilled""
  ) {
    pageInfo { hasNextPage endCursor }
    edges {
      node {
        id
        note
        shippingAddress { address1 phone }
        customer { numberOfOrders }
      }
    }
  }
}";
    }

    internal static class GraphQlMutations
    {
        public const string TagsRemove = @"
mutation ($id: ID!, $tags: [String!]!) {
  tagsRemove(id: $id, tags: $tags) {
    userErrors { message }
  }
}";
    }
}
