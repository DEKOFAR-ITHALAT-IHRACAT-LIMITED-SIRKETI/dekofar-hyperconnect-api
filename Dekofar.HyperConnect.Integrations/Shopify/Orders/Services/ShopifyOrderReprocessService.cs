using System;
using System.Threading;
using System.Threading.Tasks;
using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.GraphQl;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services
{
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

        public async Task<int> ReprocessOpenOrdersAsync(
            string shopDomain,
            CancellationToken ct)
        {
            var store = await _db.ShopifyStores
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ShopDomain == shopDomain && x.IsActive, ct);

            if (store == null)
                return 0;

            int processed = 0;
            string? cursor = null;

            while (true)
            {
                var json = await _graphQl.ExecuteAsync(
                    store.ShopDomain,
                    store.AccessToken,
                    ShopifyGraphQlQueries.OpenOrdersFull,
                    new { first = BatchSize, cursor },
                    ct);

                var orders = json["data"]?["orders"] as JObject;
                if (orders == null) break;

                var edges = orders["edges"] as JArray;
                if (edges == null || edges.Count == 0) break;

                var hasNextPage = orders["pageInfo"]?["hasNextPage"]?.Value<bool>() == true;
                var nextCursor = orders["pageInfo"]?["endCursor"]?.ToString();

                foreach (var edge in edges)
                {
                    if (edge["node"] is not JObject node)
                        continue;

                    var normalized = NormalizeGraphQlOrder(node);

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

        private static JObject NormalizeGraphQlOrder(JObject node)
        {
            return new JObject
            {
                ["admin_graphql_api_id"] = node["id"]?.ToString(),
                ["total_price"] = node["totalPriceSet"]?["shopMoney"]?["amount"]?.ToString(),
                ["line_items"] = ExtractLineItems(node),
                ["note"] = node["note"]?.ToString(),
                ["shipping_address"] = node["shippingAddress"] as JObject ?? new JObject(),
                ["customer"] = new JObject
                {
                    ["orders_count"] = node["customer"]?["numberOfOrders"]?.Value<int>() ?? 0
                },
                ["tags"] = ""
            };
        }

        private static JArray ExtractLineItems(JObject node)
        {
            var arr = new JArray();

            if (node["lineItems"]?["edges"] is not JArray edges)
                return arr;

            foreach (var e in edges)
            {
                if (e["node"] is not JObject item)
                    continue;

                arr.Add(new JObject
                {
                    ["product_id"] = item["product"]?["id"]?.ToString(),
                    ["quantity"] = item["quantity"]?.Value<int>() ?? 1
                });
            }

            return arr;
        }
    }
}
