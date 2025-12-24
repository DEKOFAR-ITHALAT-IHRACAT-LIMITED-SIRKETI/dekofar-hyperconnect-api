using Newtonsoft.Json.Linq;
using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

/// <summary>
/// Aynı telefon numarasıyla
/// EN AZ 2 ADET GÖNDERİLMEMİŞ (UNFULFILLED) sipariş varsa
/// → TÜM BU SİPARİŞLER ARA1 yapılır
///
/// Tek gönderilmemiş sipariş varsa
/// → KURALA TAKILMAZ (DHL / PTT olabilir)
///
/// Gönderilmiş (FULFILLED) siparişlere ASLA dokunulmaz
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

        // 🔍 Aynı telefonla siparişleri getir
        var query = @"
query ($query: String!) {
  orders(first: 10, query: $query) {
    edges {
      node {
        id
        displayFulfillmentStatus
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

        if (edges == null || edges.Count == 0)
            return null;

        // 🔑 SADECE GÖNDERİLMEMİŞLERİ AL
        var unfulfilledOrders = edges
            .Select(e => e["node"])
            .Where(n =>
                n?["displayFulfillmentStatus"]?.ToString()
                    .Equals("UNFULFILLED", StringComparison.OrdinalIgnoreCase) == true)
            .ToList();

        // ❗ EN AZ 2 ADET OLMALI
        if (unfulfilledOrders.Count < 2)
            return null;

        // 🔁 DİĞER GÖNDERİLMEMİŞ SİPARİŞLERİ ARA1 YAP
        foreach (var node in unfulfilledOrders)
        {
            var orderId = node?["id"]?.ToString();
            if (string.IsNullOrWhiteSpace(orderId))
                continue;

            // Kendisi hariç
            if (orderId == currentOrderId)
                continue;

            await AddAra1TagIfNotExistsAsync(
                node!,
                orderId,
                ct);
        }

        // 🔴 BU SİPARİŞ DE ARA1
        return new OrderTagResult
        {
            Tag = "ara1",
            Reason = "Aynı telefon numarasıyla birden fazla gönderilmemiş sipariş"
        };
    }

    // --------------------------------------------------
    // 🔧 YARDIMCI: ARA1 YOKSA EKLE
    // --------------------------------------------------
    private async Task AddAra1TagIfNotExistsAsync(
        JToken node,
        string orderId,
        CancellationToken ct)
    {
        var existingTags =
            node["tags"]?.ToString() ?? string.Empty;

        if (existingTags
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Any(t => t.Trim()
                .Equals("ara1", StringComparison.OrdinalIgnoreCase)))
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
