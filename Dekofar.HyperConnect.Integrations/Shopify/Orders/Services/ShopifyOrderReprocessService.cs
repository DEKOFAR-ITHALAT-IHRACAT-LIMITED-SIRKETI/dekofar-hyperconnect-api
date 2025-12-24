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

    public async Task<int> ReprocessLastDayAsync(CancellationToken ct)
    {
        var since =
            DateTime.UtcNow
                .AddDays(-1)
                .ToString("yyyy-MM-ddTHH:mm:ssZ");

        var queryString =
            $"created_at:>={since} financial_status:pending fulfillment_status:unfulfilled";

        var gql = @"
query ($query: String!) {
  orders(first: 50, query: $query) {
    edges {
      node {
        id
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
        lineItems(first: 20) {
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
            new { query = queryString },
            ct);

        var edges =
            json["data"]?["orders"]?["edges"] as JArray;

        if (edges == null || edges.Count == 0)
            return 0;

        int processed = 0;

        foreach (var edge in edges)
        {
            if (edge["node"] is not JObject gqlOrder)
                continue;

            var normalized = NormalizeGraphQlOrder(gqlOrder);

            await _autoTag.ApplyAutoTagsAsync(normalized, ct);
            processed++;
        }

        return processed;
    }

    private static JObject NormalizeGraphQlOrder(JObject node)
    {
        return new JObject
        {
            ["admin_graphql_api_id"] = node["id"],

            ["total_weight"] = node["totalWeight"],

            ["total_price"] =
                node["totalPriceSet"]?["shopMoney"]?["amount"],

            ["shipping_address"] = new JObject
            {
                ["address1"] = node["shippingAddress"]?["address1"],
                ["city"] = node["shippingAddress"]?["city"],
                ["phone"] = node["shippingAddress"]?["phone"],
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
            )
        };
    }
}
