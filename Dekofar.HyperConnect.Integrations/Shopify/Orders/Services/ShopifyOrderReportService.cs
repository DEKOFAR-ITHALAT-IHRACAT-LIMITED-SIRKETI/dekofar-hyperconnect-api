using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models.Dtos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services
{
    public class ShopifyOrderReportService
    {
        private readonly ShopifyGraphQlClient _graphQl;
        private readonly ILogger<ShopifyOrderReportService> _logger;

        public ShopifyOrderReportService(
            ShopifyGraphQlClient graphQl,
            ILogger<ShopifyOrderReportService> logger)
        {
            _graphQl = graphQl;
            _logger = logger;
        }

        // =====================================================
        // 1️⃣ AÇIK + GÖNDERİLMEMİŞ SİPARİŞLER
        // ÜRÜN → VARYANT → TOPLAM ADET (TAG OPSİYONEL)
        // =====================================================
        public async Task<List<ProductVariantSummaryDto>>
            GetOpenOrderProductSummaryAsync(
                string? tag,
                CancellationToken ct = default)
        {
            var result = new Dictionary<string, ProductVariantSummaryDto>();
            string? cursor = null;
            bool hasNextPage;

            // 🔑 TAG FILTER (Shopify native search)
            var tagFilter = string.IsNullOrWhiteSpace(tag)
                ? ""
                : $" tag:{tag.Trim()}";

            do
            {
                var gql = $@"
query ($cursor: String) {{
  orders(
    first: 50
    after: $cursor
    query: ""fulfillment_status:unfulfilled financial_status:pending{tagFilter}""
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
              sku
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

                var orderEdges = orders["edges"] as JArray;
                if (orderEdges == null)
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

                        var quantity = node["quantity"]?.Value<int>() ?? 0;
                        if (quantity <= 0)
                            continue;

                        var imageUrl = SafeGetImageUrl(node);

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
                "GRAPHQL PRODUCT REPORT → Tag={Tag}, ProductCount={Count}",
                tag ?? "(none)",
                result.Count);

            return result.Values
                .OrderByDescending(p => p.TotalQuantity)
                .ToList();
        }

        // =====================================================
        // 2️⃣ AÇIK SİPARİŞLER → ETİKET / SİPARİŞ SAYISI
        // =====================================================
        public async Task<List<OrderTagSummaryDto>>
            GetOpenOrderTagSummaryAsync(CancellationToken ct = default)
        {
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
    pageInfo {
      hasNextPage
      endCursor
    }
    edges {
      node {
        tags
      }
    }
  }
}";

                var json = await _graphQl.ExecuteAsync(
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

                var edges = orders["edges"] as JArray;
                if (edges == null)
                    continue;

                foreach (var edge in edges)
                {
                    var tagsRaw = edge["node"]?["tags"]?.ToString();

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
        // 🔒 SAFE IMAGE HELPER
        // =====================================================
        private static string? SafeGetImageUrl(JObject node)
        {
            if (node["variant"] is JObject variantObj &&
                variantObj["image"] is JObject variantImage &&
                variantImage["url"] != null)
            {
                return variantImage["url"]!.ToString();
            }

            if (node["product"] is JObject productObj &&
                productObj["featuredImage"] is JObject productImage &&
                productImage["url"] != null)
            {
                return productImage["url"]!.ToString();
            }

            return null;
        }
    }
}
