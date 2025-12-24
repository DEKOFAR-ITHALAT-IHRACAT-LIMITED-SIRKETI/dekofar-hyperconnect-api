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
        var since = DateTime.UtcNow.AddDays(-1)
            .ToString("yyyy-MM-ddTHH:mm:ssZ");

        var gql = @"
query ($query: String!) {
  orders(first: 50, query: $query) {
    edges {
      node {
        id
        createdAt
        tags
        displayFulfillmentStatus
        displayFinancialStatus
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
              product {
                id
              }
            }
          }
        }
      }
    }
  }
}";

        var query = $"created_at:>={since} financial_status:pending fulfillment_status:unfulfilled";

        var json = await _graphQl.ExecuteAsync(
            gql,
            new { query },
            ct);

        var orders = json["data"]?["orders"]?["edges"] as JArray;
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
