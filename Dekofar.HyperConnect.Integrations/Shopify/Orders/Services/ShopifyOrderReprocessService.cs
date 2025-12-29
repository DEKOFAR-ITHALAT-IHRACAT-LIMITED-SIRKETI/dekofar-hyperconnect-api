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
        string? cursor = null;
        bool hasNextPage = true;
        int processed = 0;

        while (hasNextPage)
        {
            var gql = @"
query ($cursor: String) {
  orders(
    first: 100
    after: $cursor
    query: ""financial_status:pending fulfillment_status:unfulfilled""
  ) {
    pageInfo {
      hasNextPage
      endCursor
    }
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

            var json = await _graphQl.ExecuteAsync(
                gql,
                new { cursor },
                ct);

            var ordersObj = json["data"]?["orders"] as JObject;
            if (ordersObj == null)
                break;

            hasNextPage =
                ordersObj["pageInfo"]?["hasNextPage"]?.Value<bool>() == true;

            cursor =
                ordersObj["pageInfo"]?["endCursor"]?.ToString();

            var edges = ordersObj["edges"] as JArray;
            if (edges == null || edges.Count == 0)
                continue;

            // 🔑 TELEFON SAYIMI (BU SAYFAYA ÖZEL)
            var phoneCounts = new Dictionary<string, int>();

            foreach (var edge in edges)
            {
                if (edge?["node"] is not JObject node)
                    continue;

                if (node["shippingAddress"] is not JObject shipping)
                    continue;

                var phone = shipping["phone"]?.ToString();
                if (string.IsNullOrWhiteSpace(phone))
                    continue;

                phoneCounts.TryGetValue(phone, out var c);
                phoneCounts[phone] = c + 1;
            }

            foreach (var edge in edges)
            {
                if (edge?["node"] is not JObject node)
                    continue;

                var normalized =
                    NormalizeGraphQlOrder(node, phoneCounts);

                await _autoTag.ApplyAutoTagsAsync(
                    normalized,
                    ct,
                    replaceExistingTags: true);

                processed++;
            }
        }

        return processed;
    }


    private static JObject NormalizeGraphQlOrder(
        JObject node,
        Dictionary<string, int> phoneCounts)
    {
        // ---------- SHIPPING ----------
        JObject? shipping = null;
        if (node.TryGetValue("shippingAddress", out var shippingToken))
            shipping = shippingToken as JObject;

        // ---------- CUSTOMER ----------
        JObject? customer = null;
        if (node.TryGetValue("customer", out var customerToken))
            customer = customerToken as JObject;

        var phone = shipping?["phone"]?.ToString();
        phoneCounts.TryGetValue(phone ?? "", out var repeatCount);

        // ---------- LINE ITEMS (🔥 KRİTİK DÜZELTME) ----------
        var lineItems = new JArray();

        if (node.TryGetValue("lineItems", out var lineItemsToken)
            && lineItemsToken is JObject lineItemsObj
            && lineItemsObj.TryGetValue("edges", out var edgesToken)
            && edgesToken is JArray edgesArray)
        {
            foreach (var edgeToken in edgesArray)
            {
                if (edgeToken is not JObject edgeObj)
                    continue;

                if (!edgeObj.TryGetValue("node", out var liNodeToken))
                    continue;

                if (liNodeToken is not JObject liNode)
                    continue;

                if (!liNode.TryGetValue("product", out var productToken))
                    continue;

                if (productToken is not JObject productObj)
                    continue;

                var productId = productObj["id"]?.ToString();
                if (string.IsNullOrWhiteSpace(productId))
                    continue;

                lineItems.Add(new JObject
                {
                    ["product_id"] = productId
                });
            }
        }

        // ---------- RETURN ----------
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
