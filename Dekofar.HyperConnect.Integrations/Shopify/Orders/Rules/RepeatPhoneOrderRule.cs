using Newtonsoft.Json.Linq;
using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class RepeatPhoneOrderRule : IOrderTagRule
{
    private readonly ShopifyGraphQlClient _graphQl;

    public RepeatPhoneOrderRule(
        ShopifyGraphQlClient graphQl)
    {
        _graphQl = graphQl;
    }

    public async Task<OrderTagResult?> EvaluateAsync(
        JObject order,
        CancellationToken ct)
    {
        var phone =
            order["shipping_address"]?["phone"]?.ToString();

        if (string.IsNullOrWhiteSpace(phone))
            return null;

        var gql = @"
query ($query: String!) {
  orders(first: 20, query: $query) {
    edges {
      node {
        displayFulfillmentStatus
        displayFinancialStatus
      }
    }
  }
}";

        var json = await _graphQl.ExecuteAsync(
            gql,
            new { query = $"phone:{phone}" },
            ct);

        var edges =
            json["data"]?["orders"]?["edges"] as JArray;

        if (edges == null)
            return null;

        var openCount = edges
            .Select(e => e["node"])
            .Count(n =>
                n?["displayFulfillmentStatus"]?.ToString() == "UNFULFILLED" &&
                n?["displayFinancialStatus"]?.ToString() == "PENDING");

        if (openCount < 2)
            return null;

        return new OrderTagResult
        {
            Tag = "ara1",
            Priority = 110,

            // 🔑 SMS / otomasyon
            ReasonCode = "REPEAT_PHONE",

            // 👤 İnsan
            Notes =
            {
                "Aynı telefon numarasıyla birden fazla açık sipariş mevcut"
            }
        };
    }
}
