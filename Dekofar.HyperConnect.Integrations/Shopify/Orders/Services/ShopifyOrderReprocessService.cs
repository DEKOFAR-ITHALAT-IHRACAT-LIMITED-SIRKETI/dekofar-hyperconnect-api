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
    /// Reset sonrası tüm AÇIK siparişleri yeniden etiketler
    /// ✔ 100+ sipariş destekli
    /// ✔ Reset flag yok sayılır
    /// ✔ Webhook’tan bağımsız
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

        public async Task<int> ReprocessOpenOrdersAsync(
            string shopDomain,
            CancellationToken ct)
        {
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

                var nextCursor =
                    orders["pageInfo"]?["endCursor"]?.ToString();

                var hasNextPage =
                    orders["pageInfo"]?["hasNextPage"]?.Value<bool>() == true;

                var shouldContinue =
                    hasNextPage ||
                    (!string.IsNullOrWhiteSpace(nextCursor) && nextCursor != cursor);

                cursor = nextCursor;

                foreach (var e in edges)
                {
                    if (e["node"] is not JObject node)
                        continue;

                    var normalized = new JObject
                    {
                        ["admin_graphql_api_id"] = node["id"]?.ToString(),
                        ["note"] = node["note"]?.ToString(),
                        ["shipping_address"] = node["shippingAddress"] as JObject ?? new JObject(),
                        ["tags"] = ""
                    };

                    await _autoTag.ApplyAutoTagsAsync(
                        normalized,
                        store.ShopDomain,
                        ct,
                        replaceExistingTags: false,
                        ignoreResetFlag: true);

                    processed++;
                }

                if (!shouldContinue)
                    break;

                await Task.Delay(BatchDelay, ct);
            }

            return processed;
        }
    }
}
