using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services
{
    /// <summary>
    /// Shopify Order Report Service
    /// ✔ GraphQL pagination
    /// ✔ Tag filter destekli
    /// ✔ Açık + gönderilmemiş siparişler
    /// ✔ Ürün / varyant / adet özetleri
    /// ✔ OAuth token DB’den
    /// </summary>
    public class ShopifyOrderReportService
    {
        private readonly ShopifyGraphQlClient _graphQl;
        private readonly IApplicationDbContext _db;
        private readonly ILogger<ShopifyOrderReportService> _logger;

        public ShopifyOrderReportService(
            ShopifyGraphQlClient graphQl,
            IApplicationDbContext db,
            ILogger<ShopifyOrderReportService> logger)
        {
            _graphQl = graphQl;
            _db = db;
            _logger = logger;
        }

        // =====================================================
        // 1️⃣ AÇIK + GÖNDERİLMEMİŞ SİPARİŞLER
        // ÜRÜN → VARYANT → TOPLAM ADET (TAG OPSİYONEL)
        // =====================================================
        public async Task<List<ProductVariantSummaryDto>>
            GetOpenOrderProductSummaryAsync(
                string shopDomain,
                string? tag,
                CancellationToken ct = default)
        {
            var store = await GetStoreAsync(shopDomain, ct);

            var result = new Dictionary<string, ProductVariantSummaryDto>();
            string? cursor = null;
            bool hasNextPage;

            var tagFilter = string.IsNullOrWhiteSpace(tag)
                ? string.Empty
                : $" tag:{tag.Trim()}";

            do
            {
                var gql = $@"
query ($cursor: String) {{
  orders(
    first: 50
    after: $cursor
    query: ""financial_status:pending fulfillment_status:unfulfilled{tagFilter}""
  ) {{
    pageInfo {{
      hasNextPage
      endCursor
    }}
    edges {{
      node {{
        lineItems(first: 100) {{
          edges {{
            node {{
              title
              variantTitle
              quantity
              variant {{ image {{ url }} }}
              product {{ featuredImage {{ url }} }}
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
                    new { cursor },
                    ct);

                var orders = json["data"]?["orders"] as JObject;
                if (orders == null)
                    break;

                hasNextPage =
                    orders["pageInfo"]?["hasNextPage"]?.Value<bool>() == true;

                cursor =
                    orders["pageInfo"]?["endCursor"]?.ToString();

                if (orders["edges"] is not JArray orderEdges)
                    continue;

                foreach (var orderEdge in orderEdges)
                {
                    var lineItems =
                        orderEdge["node"]?["lineItems"]?["edges"] as JArray;

                    if (lineItems == null)
                        continue;

                    foreach (var li in lineItems)
                    {
                        if (li["node"] is not JObject node)
                            continue;

                        var productTitle = node["title"]?.ToString();
                        if (string.IsNullOrWhiteSpace(productTitle))
                            continue;

                        var variantTitle =
                            string.IsNullOrWhiteSpace(node["variantTitle"]?.ToString())
                                ? "Standart"
                                : node["variantTitle"]!.ToString();

                        var quantity =
                            node["quantity"]?.Value<int>() ?? 0;

                        if (quantity <= 0)
                            continue;

                        var imageUrl = GetImageUrl(node);

                        if (!result.TryGetValue(productTitle, out var product))
                        {
                            product = new ProductVariantSummaryDto
                            {
                                ProductTitle = productTitle,
                                ProductImageUrl = imageUrl
                            };
                            result[productTitle] = product;
                        }

                        var variant = product.Variants
                            .FirstOrDefault(v => v.VariantTitle == variantTitle);

                        if (variant == null)
                        {
                            product.Variants.Add(new VariantSummaryDto
                            {
                                VariantTitle = variantTitle,
                                Quantity = quantity,
                                ImageUrl = imageUrl
                            });
                        }
                        else
                        {
                            variant.Quantity += quantity;
                        }
                    }
                }

            } while (hasNextPage);

            _logger.LogInformation(
                "SHOPIFY REPORT → Shop={Shop}, Tag={Tag}, ProductCount={Count}",
                shopDomain,
                tag ?? "(none)",
                result.Count);

            return result.Values
                .OrderByDescending(x => x.TotalQuantity)
                .ToList();
        }

        // =====================================================
        // 2️⃣ AÇIK SİPARİŞLER → ETİKET / SİPARİŞ SAYISI
        // =====================================================
        public async Task<List<OrderTagSummaryDto>>
            GetOpenOrderTagSummaryAsync(
                string shopDomain,
                CancellationToken ct = default)
        {
            var store = await GetStoreAsync(shopDomain, ct);

            var counter = new Dictionary<string, int>();
            string? cursor = null;
            bool hasNext;

            do
            {
                var gql = @"
query ($cursor: String) {
  orders(
    first: 50
    after: $cursor
    query: ""fulfillment_status:unfulfilled""
  ) {
    pageInfo { hasNextPage endCursor }
    edges {
      node { tags }
    }
  }
}";

                var json = await _graphQl.ExecuteAsync(
                    store.ShopDomain,
                    store.AccessToken,
                    gql,
                    new { cursor },
                    ct);

                var orders = json["data"]?["orders"] as JObject;
                if (orders == null)
                    break;

                hasNext =
                    orders["pageInfo"]?["hasNextPage"]?.Value<bool>() == true;

                cursor =
                    orders["pageInfo"]?["endCursor"]?.ToString();

                if (orders["edges"] is not JArray edges)
                    continue;

                foreach (var edge in edges)
                {
                    var tagsRaw =
                        edge["node"]?["tags"]?.ToString();

                    if (string.IsNullOrWhiteSpace(tagsRaw))
                    {
                        counter.TryAdd("etiketsiz", 0);
                        counter["etiketsiz"]++;
                        continue;
                    }

                    foreach (var t in tagsRaw.Split(','))
                    {
                        var key = t.Trim();
                        if (string.IsNullOrEmpty(key))
                            continue;

                        counter.TryAdd(key, 0);
                        counter[key]++;
                    }
                }

            } while (hasNext);

            return counter
                .OrderByDescending(x => x.Value)
                .Select(x => new OrderTagSummaryDto
                {
                    Tag = x.Key,
                    OrderCount = x.Value
                })
                .ToList();
        }

        // =====================================================
        // 🔒 IMAGE SAFE HELPER
        // =====================================================
        private static string? GetImageUrl(JObject node)
        {
            return
                node["variant"]?["image"]?["url"]?.ToString()
                ?? node["product"]?["featuredImage"]?["url"]?.ToString();
        }

        // =====================================================
        // 🔑 STORE HELPER
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
