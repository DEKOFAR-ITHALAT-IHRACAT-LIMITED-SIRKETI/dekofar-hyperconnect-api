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
    /// Baştan etiketler (eski etiketleri siler)
    /// </summary>
    public async Task<int> ReprocessOpenOrdersAsync(CancellationToken ct)
    {
        var gql = @"
query {
  orders(first: 100, query: ""financial_status:pending fulfillment_status:unfulfilled"") {
    edges {
      node {
        id
        tags
        note
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
      }
    }
  }
}";

        var json = await _graphQl.ExecuteAsync(gql, new { }, ct);

        var orderEdges =
            json["data"]?["orders"]?["edges"] as JArray;

        if (orderEdges == null || orderEdges.Count == 0)
            return 0;

        // 🔑 Telefon bazlı tekrar sayımı (NULL SAFE)
        var phoneCounts = orderEdges
            .Select(x => x["node"] as JObject)
            .Where(o => o != null)
            .Select(o =>
                o!["shippingAddress"]?["phone"]?.ToString())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .GroupBy(p => p!)
            .ToDictionary(g => g.Key, g => g.Count());

        int processed = 0;

        foreach (var edge in orderEdges)
        {
            if (edge["node"] is not JObject orderNode)
                continue;

            var normalized =
                NormalizeGraphQlOrder(orderNode, phoneCounts);

            await _autoTag.ApplyAutoTagsAsync(
                normalized,
                ct,
                replaceExistingTags: true);

            processed++;
        }

        return processed;
    }

    // 🔄 GRAPHQL → RULE ENGINE FORMAT
    private static JObject NormalizeGraphQlOrder(
        JObject node,
        Dictionary<string, int> phoneCounts)
    {
        var shipping = node["shippingAddress"] as JObject;
        var customer = node["customer"] as JObject;

        var phone =
            shipping?["phone"]?.ToString();

        phoneCounts.TryGetValue(
            phone ?? string.Empty,
            out var repeatCount);

        // ✅ LINE ITEMS (JVALUE SAFE)
        var lineItemsArray = new JArray();

        if (node["lineItems"]?["edges"] is JArray lineItemEdges)
        {
            foreach (var edgeItem in lineItemEdges)
            {
                if (edgeItem is not JObject edgeObj)
                    continue;

                if (edgeObj["node"] is not JObject nodeObj)
                    continue;

                var productId =
                    nodeObj["product"]?["id"]?.ToString();

                if (!string.IsNullOrWhiteSpace(productId))
                {
                    lineItemsArray.Add(new JObject
                    {
                        ["product_id"] = productId
                    });
                }
            }
        }

        return new JObject
        {
            ["admin_graphql_api_id"] =
                node["id"]?.ToString(),

            ["tags"] =
                node["tags"]?.ToString(),

            ["note"] =
                node["note"]?.ToString(),

            ["total_weight"] =
                node["totalWeight"]?.Value<decimal?>(),

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

            ["line_items"] = lineItemsArray,

            // ⭐ RULE METADATA
            ["__repeat_phone_count"] = repeatCount
        };
    }
}
