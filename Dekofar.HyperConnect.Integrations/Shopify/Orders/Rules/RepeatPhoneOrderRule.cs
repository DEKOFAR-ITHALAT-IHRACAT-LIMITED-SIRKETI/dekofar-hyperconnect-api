using Newtonsoft.Json.Linq;
using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Rules;

/// <summary>
/// Aynı telefon numarasıyla
/// 2+ AÇIK + GÖNDERİLMEMİŞ + ÖDEME BEKLEYEN sipariş varsa
/// → HER İKİSİ DE ARA1
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

        var gql = @"
query ($query: String!) {
  orders(first: 20, query: $query) {
    edges {
      node {
        id
        displayFulfillmentStatus
        displayFinancialStatus
        tags
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

        // ✅ GERÇEK AÇIK + GÖNDERİLMEMİŞ + BEKLEYENLER
        var validOrderIds = edges
            .Select(e => e["node"])
            .Where(n =>
                n?["displayFulfillmentStatus"]?.ToString() == "UNFULFILLED" &&
                n?["displayFinancialStatus"]?.ToString() == "PENDING")
            .Select(n => n?["id"]?.ToString())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct() // 🔴 KRİTİK
            .ToList();

        // ❗ GERÇEKTEN 2 SİPARİŞ YOKSA → HİÇBİR ŞEY YAPMA
        if (validOrderIds.Count < 2)
            return null;

        // 🔁 DİĞER AÇIK SİPARİŞLERİ ARA1 YAP
        foreach (var orderId in validOrderIds)
        {
            if (orderId == currentOrderId)
                continue;

            await AddAra1TagIfNotExistsAsync(orderId!, ct);
        }

        // 🔴 BU SİPARİŞ DE ARA1
        return new OrderTagResult
        {
            Tag = "ara1",
            Reason = "Aynı telefon numarasıyla birden fazla açık ve gönderilmemiş sipariş"
        };
    }

    private async Task AddAra1TagIfNotExistsAsync(
        string orderId,
        CancellationToken ct)
    {
        var mutation = @"
mutation ($id: ID!, $tags: [String!]!) {
  tagsAdd(id: $id, tags: $tags) {
    userErrors { message }
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
