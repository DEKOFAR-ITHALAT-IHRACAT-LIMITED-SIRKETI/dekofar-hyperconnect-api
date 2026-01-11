using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.GraphQl;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services
{
    /// <summary>
    /// Reset sonrası yeniden etiketleme
    /// </summary>
    public sealed class ShopifyOrderReprocessService
    {
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

            var json = await _graphQl.ExecuteAsync(
                store.ShopDomain,
                store.AccessToken,
                ShopifyGraphQlQueries.OpenOrdersFull,
                new { first = 100 },
                ct);

            var edges = json["data"]?["orders"]?["edges"] as JArray;
            if (edges == null)
                return 0;

            int processed = 0;

            foreach (var e in edges)
            {
                var node = e["node"] as JObject;
                if (node == null)
                    continue;

                var normalized = new JObject
                {
                    ["admin_graphql_api_id"] = node["id"],
                    ["note"] = node["note"],
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

            return processed;
        }
    }
}
