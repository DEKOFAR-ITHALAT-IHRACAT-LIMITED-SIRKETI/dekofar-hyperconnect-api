using Newtonsoft.Json.Linq;
using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

public class RepeatPhoneOrderRule : IOrderTagRule
{
    private readonly ShopifyGraphQlClient _graphQl;

    public RepeatPhoneOrderRule(ShopifyGraphQlClient graphQl)
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

        var currentOrderId =
            order["admin_graphql_api_id"]?.ToString();

        if (string.IsNullOrWhiteSpace(currentOrderId))
            return null;

        var query = @"
query ($query: String!) {
  orders(first: 20, query: $query) {
    edges {
      node {
        id
        displayFulfillmentStatus
        financialStatus
        tags
      }
    }
  }
}";

        var json = await _graphQl.ExecuteAsync(
            query,
            new { query = $"phone:{phone}" },
            ct);

        var edges =
            json["data"]?["orders"]?["edges"] as JArray;

        if (edges == null)
            return null;

        // ✅ TEKİL + GERÇEK GÖNDERİLMEMİŞ SİPARİŞLER
        var unfulfilledOrderIds = edges
            .Select(e => e["node"])
            .Where(n =>
                n?["displayFulfillmentStatus"]?.ToString() == "UNFULFILLED" &&
                n?["financialStatus"]?.ToString() == "PENDING")
            .Select(n => n?["id"]?.ToString())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct() // 🔴 KRİTİK SATIR
            .ToList();

        // ❗ GERÇEKTEN 2 FARKLI SİPARİŞ YOKSA ÇIK
        if (unfulfilledOrderIds.Count < 2)
            return null;

        // 🔁 DİĞER SİPARİŞLERİ ARA1 YAP
        foreach (var orderId in unfulfilledOrderIds)
        {
            if (orderId == currentOrderId)
                continue;

            await AddAra1TagAsync(orderId!, ct);
        }

        return new OrderTagResult
        {
            Tag = "ara1",
            Reason = "Aynı telefon numarasıyla birden fazla açık ve gönderilmemiş sipariş"
        };
    }

    private async Task AddAra1TagAsync(
        string orderId,
        CancellationToken ct)
    {
        var mutation = @"
mutation ($id: ID!, $tags: [String!]!) {
  tagsAdd(id: $id, tags: $tags) {
    userErrors {
      message
    }
  }
}";

        await _graphQl.ExecuteAsync(
            mutation,
            new
            {
                id = orderId,
                tags = new[] { "ara1" }
            },
            ct);
    }
}
