using Newtonsoft.Json.Linq;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

/// <summary>
/// Aynı telefon numarasıyla birden fazla GÖNDERİLMEMİŞ sipariş varsa
/// → HER İKİSİNİ DE ARA1 yapar
/// Gönderilmiş (fulfilled) siparişlere ASLA dokunmaz
/// </summary>
public class RepeatPhoneOrderRule : IOrderTagRule
{
    private readonly ShopifyGraphQlClient _graphQl;

    public RepeatPhoneOrderRule(ShopifyGraphQlClient graphQl)
    {
        _graphQl = graphQl;
    }

    public async Task<OrderTagResult?> EvaluateAsync(JObject order, CancellationToken ct)
    {
        var phone =
            order["shipping_address"]?["phone"]?.ToString();

        if (string.IsNullOrWhiteSpace(phone))
            return null;

        var currentOrderId =
            order["admin_graphql_api_id"]?.ToString();

        if (string.IsNullOrWhiteSpace(currentOrderId))
            return null;

        // 🔍 Aynı telefonla son siparişleri getir
        var query = @"
query ($query: String!) {
  orders(first: 10, query: $query) {
    edges {
      node {
        id
        fulfillmentStatus
        tags
      }
    }
  }
}";

        var searchQuery = $"phone:{phone}";

        var json = await _graphQl.ExecuteAsync(
            query,
            new { query = searchQuery },
            ct);

        var edges =
            json["data"]?["orders"]?["edges"] as JArray;

        if (edges == null || edges.Count <= 1)
            return null; // tek sipariş → sorun yok

        bool hasAnotherUnfulfilled = false;

        // 🔁 ESKİ SİPARİŞLERİ KONTROL ET
        foreach (var edge in edges)
        {
            var node = edge["node"];
            if (node == null)
                continue;

            var orderId = node["id"]?.ToString();
            if (string.IsNullOrWhiteSpace(orderId))
                continue;

            // Kendisi ise atla
            if (orderId == currentOrderId)
                continue;

            var fulfillmentStatus =
                node["fulfillmentStatus"]?.ToString();

            // ❌ Gönderilmiş siparişlere ASLA dokunma
            if (!string.Equals(
                    fulfillmentStatus,
                    "UNFULFILLED",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            hasAnotherUnfulfilled = true;

            // 🟠 Eski ama GÖNDERİLMEMİŞ siparişi ARA1 yap
            await AddAra1TagIfNotExistsAsync(node, orderId, ct);
        }

        // Eğer başka unfulfilled yoksa → bu sipariş de normal devam eder
        if (!hasAnotherUnfulfilled)
            return null;

        // 🔴 BU SİPARİŞ DE ARA1
        return new OrderTagResult
        {
            Tag = "ara1",
            Reason = "Aynı telefon numarasıyla gönderilmemiş tekrar sipariş"
        };
    }

    private async Task AddAra1TagIfNotExistsAsync(
        JToken node,
        string orderId,
        CancellationToken ct)
    {
        var existingTags =
            node["tags"]?.ToString() ?? "";

        if (existingTags
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Any(t => t.Trim().Equals("ara1", StringComparison.OrdinalIgnoreCase)))
        {
            return; // zaten ara1 varsa tekrar ekleme
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
