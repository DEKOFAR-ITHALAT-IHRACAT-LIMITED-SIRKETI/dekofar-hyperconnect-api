using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.Constants;
using Dekofar.HyperConnect.Integrations.Shopify.GraphQl;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services
{
    public sealed class ShopifyOrderResetService
    {
        private const int BatchSize = 100;
        private static readonly TimeSpan BatchDelay = TimeSpan.FromSeconds(2);

        private readonly ShopifyGraphQlClient _graphQl;
        private readonly IApplicationDbContext _db;

        public ShopifyOrderResetService(
            ShopifyGraphQlClient graphQl,
            IApplicationDbContext db)
        {
            _graphQl = graphQl;
            _db = db;
        }

        public async Task<int> ResetAllOpenOrderTagsAsync(
            string shopDomain,
            CancellationToken ct)
        {
            var store = await _db.ShopifyStores
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ShopDomain == shopDomain && x.IsActive, ct);

            if (store == null)
                throw new InvalidOperationException("Shop not found");

            int cleared = 0;
            string? cursor = null;

            while (true)
            {
                var json = await _graphQl.ExecuteAsync(
                    store.ShopDomain,
                    store.AccessToken,
                    ShopifyGraphQlQueries.OpenOrdersMinimal,
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

                    var orderId = node["id"]?.ToString();
                    if (string.IsNullOrWhiteSpace(orderId))
                        continue;

                    // ✅ TAGS STRING OLARAK OKUNUR
                    var tagsRaw = node["tags"]?.ToString();
                    if (string.IsNullOrWhiteSpace(tagsRaw))
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
                        ShopifyGraphQlMutations.TagsRemove,
                        new { id = orderId, tags },
                        ct);

                    await _graphQl.ExecuteAsync(
                        store.ShopDomain,
                        store.AccessToken,
                        ShopifyGraphQlMutations.UpdateOrderNote,
                        new { id = orderId, note = ShopifySystemNotes.ResetFlag },
                        ct);

                    cleared++;
                }

                if (!hasNextPage || string.IsNullOrWhiteSpace(nextCursor))
                    break;

                cursor = nextCursor;
                await Task.Delay(BatchDelay, ct);
            }

            return cleared;
        }
    }
}
