using Newtonsoft.Json.Linq;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

/// <summary>
/// Aynı telefon numarasıyla
/// GÖNDERİLMEMİŞ (unfulfilled) birden fazla sipariş varsa
/// → HER İKİ SİPARİŞ DE ara1 olur
/// </summary>
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

        // ✅ SADECE UNFULFILLED SİPARİŞLER
        var searchQuery =
            $"phone:{phone} fulfillment_status:unfulfilled";

        var gql = @"
query ($query: String!) {
  orders(first: 10, query: $query) {
    edges {
      node {
        id
        tags
      }
    }
  }
}";

        var json = await _graphQl.ExecuteAsync(
            gql,
            new { query = searchQuery },
            ct);

        var edges =
            json["data"]?["orders"]?["edges"] as JArray;

        // Tek sipariş varsa → tekrar yok
        if (edges == null || edges.Count <= 1)
            return null;

        bool anotherUnfulfilledExists = false;

        foreach (var edge in edges)
        {
            var node = edge["node"];
            if (node == null)
                continue;

            var orderId = node["id"]?.ToString();
            if (string.IsNullOrWhiteSpace(orderId))
                continue;

            // Kendisi değilse → diğer unfulfilled sipariş
            if (!orderId.Equals(currentOrderId, StringComparison.OrdinalIgnoreCase))
            {
                anotherUnfulfilledExists = true;

                // 🔁 ESKİ UNFULFILLED SİPARİŞE ara1 EKLE
                await AddAra1IfMissingAsync(orderId, node, ct);
            }
        }

        if (!anotherUnfulfilledExists)
            return null;

        // 🔴 BU SİPARİŞ DE ara1
        return new OrderTagResult
        {
            Tag = "ara1",
            Reason = "Aynı telefon numarasıyla gönderilmemiş tekrar sipariş"
        };
    }

    private async Task AddAra1IfMissingAsync(
        string orderId,
        JToken node,
        CancellationToken ct)
    {
        var tagsRaw = node["tags"]?.ToString() ?? "";

        if (tagsRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Any(t => t.Trim().Equals("ara1", StringComparison.OrdinalIgnoreCase)))
        {
            return; // zaten var
        }

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
