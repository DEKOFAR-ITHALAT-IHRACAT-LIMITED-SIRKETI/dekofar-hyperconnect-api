using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;
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
    /// Son 1 gün içindeki
    /// - ödeme beklemede
    /// - gönderilmemiş
    /// siparişleri yeniden etiketler
    /// </summary>
    public async Task<int> ReprocessLastDayAsync(CancellationToken ct)
    {
        var gql = @"
query {
  orders(
    first: 50
    query: ""created_at:>=-1d financial_status:pending fulfillment_status:unfulfilled""
  ) {
    edges {
      node {
        admin_graphql_api_id
        total_price
        total_weight
        tags
        created_at
        shipping_address {
          address1
          city
          phone
          country_code
        }
        customer {
          orders_count
        }
        line_items {
          product_id
        }
      }
    }
  }
}";

        var json = await _graphQl.ExecuteAsync(gql, null, ct);

        var orders =
            json["data"]?["orders"]?["edges"] as JArray;

        if (orders == null)
            return 0;

        int count = 0;

        foreach (var edge in orders)
        {
            if (edge["node"] is JObject order)
            {
                await _autoTag.ApplyAutoTagsAsync(order, ct);
                count++;
            }
        }

        return count;
    }
}
