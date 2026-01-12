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
    /// Shopify Open Orders TAG RESET Service
    /// ✔ SADECE manuel (Swagger)
    /// ✔ Webhook’tan tamamen bağımsız
    /// ✔ Açık siparişlerde TÜM tag’leri siler
    /// ✔ Reset flag note’a eklenir
    /// ✔ Shopify pagination bug-safe
    /// </summary>
    public sealed class ShopifyOrderResetService
    {
        private readonly ShopifyGraphQlClient _graphQl;
        private readonly IApplicationDbContext _db;

        private const int BatchSize = 100;
        private static readonly TimeSpan BatchDelay = TimeSpan.FromSeconds(2);

        public ShopifyOrderResetService(
            ShopifyGraphQlClient graphQl,
            IApplicationDbContext db)
        {
            _graphQl = graphQl;
            _db = db;
        }

        // =====================================================
        // 🚀 ENTRY (Swagger çağırır)
        // =====================================================
        public async Task<int> ResetAllOpenOrderTagsAsync(
            string shopDomain,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(shopDomain))
                throw new InvalidOperationException("shopDomain is required");

            var store = await GetStoreAsync(shopDomain, ct);

            int clearedCount = 0;
            string? cursor = null;

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                var json = await _graphQl.ExecuteAsync(
                    store.ShopDomain,
                    store.AccessToken,
                    ShopifyGraphQlQueries.OpenOrdersMinimal,
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

                // 🔥 SHOPIFY BUG SAFE CONTINUE
                var shouldContinue =
                    hasNextPage ||
                    (!string.IsNullOrWhiteSpace(nextCursor) &&
                     nextCursor != cursor);

                cursor = nextCursor;

                foreach (var edge in edges)
                {
                    if (edge["node"] is not JObject node)
                        continue;

                    var orderId = node["id"]?.ToString();
                    if (string.IsNullOrWhiteSpace(orderId))
                        continue;

                    var tagsArray = node["tags"] as JArray;
                    if (tagsArray == null || tagsArray.Count == 0)
                        continue;

                    var tags = tagsArray
                        .Select(t => t.ToString())
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .ToArray();

                    if (tags.Length == 0)
                        continue;

                    // 🧹 TAG SİL
                    await _graphQl.ExecuteAsync(
                        store.ShopDomain,
                        store.AccessToken,
                        ShopifyGraphQlMutations.TagsRemove,
                        new { id = orderId, tags },
                        ct);

                    // 📝 RESET FLAG (webhook çakışmasını önler)
                    await _graphQl.ExecuteAsync(
                        store.ShopDomain,
                        store.AccessToken,
                        ShopifyGraphQlMutations.UpdateOrderNote,
                        new
                        {
                            id = orderId,
                            note = ShopifySystemNotes.ResetFlag
                        },
                        ct);

                    clearedCount++;
                }

                if (!shouldContinue)
                    break;

                await Task.Delay(BatchDelay, ct);
            }

            return clearedCount;
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
