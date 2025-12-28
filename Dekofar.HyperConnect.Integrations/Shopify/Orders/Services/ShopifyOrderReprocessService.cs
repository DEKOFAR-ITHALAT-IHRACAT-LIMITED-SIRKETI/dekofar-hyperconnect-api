using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;

public class ShopifyOrderReprocessService
{
    private readonly ShopifyGraphQlClient _graphQl;
    private readonly ShopifyOrderAutoTagService _autoTag;

    public ShopifyOrderReprocessService(
        ShopifyGraphQlClient graphQl,
        ShopifyOrderAutoTagService autoTag)
    {
        _graphQl = graphQl;
        _autoTag = autoTag;
    }

    /// <summary>
    /// TÜM açık + ödeme bekleyen + gönderilmemiş siparişleri
    /// BAŞTAN etiketler (eski etiketleri siler)
    /// </summary>
    public async Task<int> ReprocessOpenOrdersAsync(CancellationToken ct)
    {
        var queryString =
            "financial_status:pending fulfillment_status:unfulfilled";

        var gql = @"
query ($query: String!) {
  orders(first: 100, query: $query) {
    edges {
      node {
        id
        tags
        totalWeight
        totalPriceSet {
          shopMoney { amount }
        }
        shippingAddress {
          address1
          city
          phone
          countryCode
        }
        customer {
          numberOfOrders
        }
        lineItems(first: 50) {
          edges {
            node {
              product { id }
            }
          }
        }
        note
      }
    }
  }
}";

        var json = await _graphQl.ExecuteAsync(
            gql,
            new { query = queryString },
            ct);

        var edges =
            json["data"]?["orders"]?["edges"] as JArray;

        if (edges == null || edges.Count == 0)
            return 0;

        // 🔑 TELEFON BAZLI SAYIM (NULL SAFE)
        var phoneCounts = edges
            .Select(e => e["node"] as JObject)
            .Where(o => o != null)
            .GroupBy(o =>
                o!["shippingAddress"] is JObject addr
                    ? addr["phone"]?.ToString()
                    : null
            )
            .Where(g => !string.IsNullOrWhiteSpace(g.Key))
            .ToDictionary(
                g => g.Key!,
                g => g.Count()
            );

        int processed = 0;

        foreach (var edge in edges)
        {
            if (edge["node"] is not JObject gqlOrder)
                continue;

            var normalized =
                NormalizeGraphQlOrder(gqlOrder, phoneCounts);

            await _autoTag.ApplyAutoTagsAsync(
                normalized,
                ct,
                replaceExistingTags: true);

            processed++;
        }

        return processed;
    }

    // ======================================================
    // GraphQL → RULE ENGINE FORMAT
    // ======================================================
    private static JObject NormalizeGraphQlOrder(
        JObject node,
        Dictionary<string, int> phoneCounts)
    {
        var phone =
            node["shippingAddress"]?["phone"]?.ToString();

        phoneCounts.TryGetValue(
            phone ?? string.Empty,
            out var repeatCount);

        return new JObject
        {
            ["admin_graphql_api_id"] = node["id"],
            ["tags"] = node["tags"],
            ["note"] = node["note"],

            ["total_weight"] = node["totalWeight"],

            ["total_price"] =
                node["totalPriceSet"]?["shopMoney"]?["amount"],

            ["shipping_address"] = new JObject
            {
                ["address1"] = node["shippingAddress"]?["address1"],
                ["city"] = node["shippingAddress"]?["city"],
                ["phone"] = phone,
                ["country_code"] = node["shippingAddress"]?["countryCode"]
            },

            ["customer"] = new JObject
            {
                ["orders_count"] =
                    node["customer"]?["numberOfOrders"]
            },

            ["line_items"] = new JArray(
                node["lineItems"]?["edges"]?
                    .Select(e => new JObject
                    {
                        ["product_id"] =
                            e["node"]?["product"]?["id"]
                    }) ?? Enumerable.Empty<JObject>()
            ),

            // ⭐ RULE METADATA
            ["__repeat_phone_count"] = repeatCount
        };
    }
}
