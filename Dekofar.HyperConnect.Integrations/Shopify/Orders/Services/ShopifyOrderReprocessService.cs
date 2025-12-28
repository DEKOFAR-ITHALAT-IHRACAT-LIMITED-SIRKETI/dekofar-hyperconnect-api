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
        totalPriceSet { shopMoney { amount } }
        shippingAddress {
          address1
          city
          phone
          countryCode
        }
        customer { numberOfOrders }
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
        var edges = json["data"]?["orders"]?["edges"] as JArray;

        if (edges == null || edges.Count == 0)
            return 0;

        // 🔑 TELEFON SAYIMI – %100 SAFE
        var phoneCounts = new Dictionary<string, int>();

        foreach (var e in edges)
        {
            if (e?["node"] is not JObject node)
                continue;

            if (node["shippingAddress"] is not JObject shipping)
                continue;

            var phone = shipping["phone"]?.ToString();
            if (string.IsNullOrWhiteSpace(phone))
                continue;

            phoneCounts.TryGetValue(phone, out var count);
            phoneCounts[phone] = count + 1;
        }

        int processed = 0;

        foreach (var e in edges)
        {
            if (e?["node"] is not JObject node)
                continue;

            var normalized =
                NormalizeGraphQlOrder(node, phoneCounts);

            await _autoTag.ApplyAutoTagsAsync(
                normalized,
                ct,
                replaceExistingTags: true);

            processed++;
        }

        return processed;
    }

    private static JObject NormalizeGraphQlOrder(
        JObject node,
        Dictionary<string, int> phoneCounts)
    {
        var shipping = node["shippingAddress"] as JObject;
        var customer = node["customer"] as JObject;

        var phone = shipping?["phone"]?.ToString();
        phoneCounts.TryGetValue(phone ?? "", out var repeatCount);

        var lineItems = new JArray();

        if (node["lineItems"]?["edges"] is JArray edges)
        {
            foreach (var edge in edges)
            {
                if (edge?["node"]?["product"]?["id"] == null)
                    continue;

                lineItems.Add(new JObject
                {
                    ["product_id"] =
                        edge["node"]!["product"]!["id"]!.ToString()
                });
            }
        }

        return new JObject
        {
            ["admin_graphql_api_id"] = node["id"]?.ToString(),
            ["tags"] = node["tags"]?.ToString(),
            ["note"] = node["note"]?.ToString(),

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

            ["line_items"] = lineItems,

            ["__repeat_phone_count"] = repeatCount
        };
    }
}
