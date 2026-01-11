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
    /// <summary>
    /// SADECE manuel reset
    /// </summary>
    public sealed class ShopifyOrderResetService
    {
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
                return 0;

            var json = await _graphQl.ExecuteAsync(
                store.ShopDomain,
                store.AccessToken,
                ShopifyGraphQlQueries.OpenOrdersMinimal,
                new { first = 100 },
                ct);

            var edges = json["data"]?["orders"]?["edges"] as JArray;
            if (edges == null)
                return 0;

            int count = 0;

            foreach (var e in edges)
            {
                var node = e["node"] as JObject;
                var id = node?["id"]?.ToString();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                var tags = node["tags"] as JArray;
                if (tags is { Count: > 0 })
                {
                    await _graphQl.ExecuteAsync(
                        store.ShopDomain,
                        store.AccessToken,
                        ShopifyGraphQlMutations.TagsRemove,
                        new { id, tags = tags.Select(t => t.ToString()).ToArray() },
                        ct);
                }

                await _graphQl.ExecuteAsync(
                    store.ShopDomain,
                    store.AccessToken,
                    ShopifyGraphQlMutations.UpdateOrderNote,
                    new { id, note = ShopifySystemNotes.ResetFlag },
                    ct);

                count++;
            }

            return count;
        }
    }
}
