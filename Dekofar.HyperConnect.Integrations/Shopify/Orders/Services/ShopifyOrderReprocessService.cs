using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services
{
    public class ShopifyOrderReprocessService
    {
        private readonly ShopifyGraphQlClient _graphQl;
        private readonly ShopifyOrderAutoTagService _autoTag;
        private readonly IApplicationDbContext _db;

        private const int BatchSize = 200;
        private static readonly TimeSpan BatchDelay = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan ConsistencyDelay = TimeSpan.FromMinutes(1);

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
        // 🚀 PROD – TÜM AÇIK SİPARİŞLER
        // =====================================================
        public async Task<int> ReprocessOpenOrdersAsync(
            string shopDomain,
            CancellationToken ct)
        {
            var store = await GetStoreAsync(shopDomain, ct);

            await Task.Delay(ConsistencyDelay, ct);

            return await ReprocessInternalAsync(
                store,
                ct,
                limit: null);
        }

        // =====================================================
        // 🧪 TEST – SADECE N SİPARİŞ
        // =====================================================
        public async Task<int> ReprocessOpenOrdersTestAsync(
            string shopDomain,
            int limit,
            CancellationToken ct)
        {
            if (limit <= 0 || limit > 50)
                throw new InvalidOperationException("limit must be between 1 and 50");

            var store = await GetStoreAsync(shopDomain, ct);

            return await ReprocessInternalAsync(
                store,
                ct,
                limit);
        }

        // =====================================================
        // 🔁 ORTAK İŞLEYİCİ
        // =====================================================
        private async Task<int> ReprocessInternalAsync(
            ShopifyStore store,
            CancellationToken ct,
            int? limit)
        {
            var gql = $@"
query {{
  orders(
    first: {(limit ?? BatchSize)}
    query: ""financial_status:pending fulfillment_status:unfulfilled""
  ) {{
    edges {{
      node {{
        id
        tags
        note
        totalPriceSet {{ shopMoney {{ amount }} }}
        shippingAddress {{ address1 city phone countryCode }}
        customer {{ numberOfOrders }}
        lineItems(first: 50) {{
          edges {{
            node {{
              quantity
              product {{ id }}
            }}
          }}
        }}
      }}
    }}
  }}
}}";

            var json = await _graphQl.ExecuteAsync(
                store.ShopDomain,
                store.AccessToken,
                gql,
                null,
                ct);

            if (json?["data"]?["orders"] is not JObject ordersObj)
                return 0;

            if (ordersObj["edges"] is not JArray edges || edges.Count == 0)
                return 0;

            // 📞 TELEFON TEKRAR SAYACI
            var phoneCounts = new Dictionary<string, int>();

            foreach (var edge in edges)
            {
                var phone =
                    edge?["node"]?["shippingAddress"]?["phone"]?.ToString();

                if (string.IsNullOrWhiteSpace(phone))
                    continue;

                phoneCounts.TryGetValue(phone, out var c);
                phoneCounts[phone] = c + 1;
            }

            int processed = 0;

            foreach (var edge in edges)
            {
                if (edge?["node"] is not JObject node)
                    continue;

                try
                {
                    var normalized =
                        NormalizeGraphQlOrder(node, phoneCounts);

                    await _autoTag.ApplyAutoTagsAsync(
                        normalized,
                        store.ShopDomain,
                        ct,
                        replaceExistingTags: true);

                    processed++;
                }
                catch
                {
                    // ❗ tek sipariş bozuksa devam
                    continue;
                }
            }

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
            phoneCounts.TryGetValue(phone ?? "", out var repeat);

            var lineItems = new JArray();

            if (node["lineItems"]?["edges"] is JArray edges)
            {
                foreach (var e in edges)
                {
                    if (e?["node"] is not JObject item)
                        continue;

                    var productId =
                        item["product"]?["id"]?.ToString();

                    var quantity =
                        item["quantity"]?.Value<int>() ?? 1;

                    if (!string.IsNullOrWhiteSpace(productId))
                    {
                        lineItems.Add(new JObject
                        {
                            ["product_id"] = productId,
                            ["quantity"] = quantity
                        });
                    }
                }
            }

            return new JObject
            {
                ["admin_graphql_api_id"] = node["id"]?.ToString(),
                ["tags"] = node["tags"]?.ToString(),
                ["note"] = node["note"]?.ToString(),
                ["total_price"] =
                    node["totalPriceSet"]?["shopMoney"]?["amount"]?.ToString(),
                ["shipping_address"] = new JObject
                {
                    ["address1"] = shipping?["address1"]?.ToString(),
                    ["city"] = shipping?["city"]?.ToString(),
                    ["phone"] = phone,
                    ["country_code"] = shipping?["countryCode"]?.ToString()
                },
                ["customer"] = new JObject
                {
                    ["orders_count"] =
                        customer?["numberOfOrders"]?.Value<int>() ?? 0
                },
                ["line_items"] = lineItems,
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
}
