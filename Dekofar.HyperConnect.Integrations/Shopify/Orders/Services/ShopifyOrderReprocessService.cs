using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.GraphQl;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services
{
    /// <summary>
    /// Reset sonrası TÜM AÇIK siparişleri yeniden etiketler
    /// ✔ 100+ sipariş (cursor pagination)
    /// ✔ Reset flag YOK SAYILIR
    /// ✔ Webhook’tan tamamen bağımsız
    /// ✔ OrderDecisionEngine ile %100 UYUMLU JSON üretir
    /// </summary>
    public sealed class ShopifyOrderReprocessService
    {
        private const int BatchSize = 100;
        private static readonly TimeSpan BatchDelay = TimeSpan.FromSeconds(2);

        private readonly ShopifyOrderAutoTagService _autoTag;
        private readonly ShopifyGraphQlClient _graphQl;
        private readonly IApplicationDbContext _db;

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
        // 🚀 ENTRY (Swagger çağırır)
        // =====================================================
        public async Task<int> ReprocessOpenOrdersAsync(
            string shopDomain,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(shopDomain))
                throw new InvalidOperationException("shopDomain is required");

            var store = await _db.ShopifyStores
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.ShopDomain == shopDomain && x.IsActive,
                    ct);

            if (store == null)
                return 0;

            int processed = 0;
            string? cursor = null;

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                var json = await _graphQl.ExecuteAsync(
                    store.ShopDomain,
                    store.AccessToken,
                    ShopifyGraphQlQueries.OpenOrdersFull,
                    new { first = BatchSize, cursor },
                    ct);

                var orders = json["data"]?["orders"] as JObject;
                if (orders == null)
                    break;

                var edges = orders["edges"] as JArray;
                if (edges == null || edges.Count == 0)
                    break;

                var hasNextPage =
                    orders["pageInfo"]?["hasNextPage"]?.Value<bool>() == true;

                var nextCursor =
                    orders["pageInfo"]?["endCursor"]?.ToString();

                foreach (var edge in edges)
                {
                    if (edge["node"] is not JObject node)
                        continue;

                    var normalized = NormalizeGraphQlOrder(node);

                    // 🔥 RESET FLAG YOK SAYILARAK KURALLAR ÇALIŞTIRILIR
                    await _autoTag.ApplyAutoTagsAsync(
                        normalized,
                        store.ShopDomain,
                        ct,
                        replaceExistingTags: false,
                        ignoreResetFlag: true);

                    processed++;
                }

                if (!hasNextPage || string.IsNullOrWhiteSpace(nextCursor))
                    break;

                cursor = nextCursor;

                await Task.Delay(BatchDelay, ct);
            }

            return processed;
        }

        // =====================================================
        // 🔄 GRAPHQL → WEBHOOK FORMAT NORMALIZE
        // =====================================================
        private static JObject NormalizeGraphQlOrder(JObject node)
        {
            var shipping = node["shippingAddress"] as JObject;
            var customer = node["customer"] as JObject;

            return new JObject
            {
                // 🔑 ID
                ["admin_graphql_api_id"] = node["id"]?.ToString(),

                // 🔴 EN KRİTİK: TOPLAM TUTAR (1000 TL kuralı için)
                ["total_price"] =
                    node["totalPriceSet"]?["shopMoney"]?["amount"]?.ToString(),

                // 🔴 LINE ITEMS (DecisionEngine kullanıyor)
                ["line_items"] = ExtractLineItems(node),

                // 📝 NOT
                ["note"] = node["note"]?.ToString(),

                // 🚚 ADRES
                ["shipping_address"] = new JObject
                {
                    ["address1"] = shipping?["address1"]?.ToString(),
                    ["city"] = shipping?["city"]?.ToString(),
                    ["phone"] = shipping?["phone"]?.ToString(),
                    ["country_code"] = shipping?["countryCode"]?.ToString()
                },

                // 👤 CUSTOMER
                ["customer"] = new JObject
                {
                    ["orders_count"] =
                        customer?["numberOfOrders"]?.Value<int>() ?? 0
                },

                // 🏷️ TAGLER BAŞLANGIÇTA BOŞ
                ["tags"] = ""
            };
        }

        // =====================================================
        // 📦 LINE ITEMS NORMALIZE
        // =====================================================
        private static JArray ExtractLineItems(JObject node)
        {
            var result = new JArray();

            if (node["lineItems"]?["edges"] is not JArray edges)
                return result;

            foreach (var e in edges)
            {
                if (e["node"] is not JObject item)
                    continue;

                result.Add(new JObject
                {
                    ["product_id"] = item["product"]?["id"]?.ToString(),
                    ["quantity"] = item["quantity"]?.Value<int>() ?? 1
                });
            }

            return result;
        }
    }
}
