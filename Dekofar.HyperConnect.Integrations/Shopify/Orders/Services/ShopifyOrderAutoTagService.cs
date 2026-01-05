using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Decisions;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;

public class ShopifyOrderAutoTagService
{
    private readonly ShopifyGraphQlClient _graphQl;
    private readonly OrderDecisionEngine _decisionEngine;

    public ShopifyOrderAutoTagService(
        ShopifyGraphQlClient graphQl,
        OrderDecisionEngine decisionEngine)
    {
        _graphQl = graphQl;
        _decisionEngine = decisionEngine;
    }

    public async Task ApplyAutoTagsAsync(
        JObject order,
        CancellationToken ct,
        bool replaceExistingTags)
    {
        var orderId = order["admin_graphql_api_id"]?.ToString();
        if (string.IsNullOrWhiteSpace(orderId))
            return;

        // =====================================================
        // 🧠 KARAR
        // =====================================================
        var decision = _decisionEngine.Decide(order);

        var tag = decision.Decision switch
        {
            OrderDecision.Automatic => "dhl",
            OrderDecision.ApprovalNeeded => "ara1",
            OrderDecision.Cancelled => "iptal",
            _ => null
        };

        if (string.IsNullOrWhiteSpace(tag))
            return;

        // =====================================================
        // 🧹 TÜM ESKİ TAGLERİ TEMİZLE
        // =====================================================
        if (replaceExistingTags)
        {
            var existingTags = order["tags"]?.ToString();

            var tagsToRemove = existingTags?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLowerInvariant())
                .Distinct()
                .ToArray();

            if (tagsToRemove is { Length: > 0 })
            {
                await _graphQl.ExecuteAsync(
                    @"mutation ($id: ID!, $tags: [String!]!) {
                      tagsRemove(id: $id, tags: $tags) {
                        userErrors { message }
                      }
                    }",
                    new { id = orderId, tags = tagsToRemove },
                    ct);
            }
        }

        // =====================================================
        // 🏷️ YENİ TAG EKLE
        // =====================================================
        await _graphQl.ExecuteAsync(
            @"mutation ($id: ID!, $tags: [String!]!) {
              tagsAdd(id: $id, tags: $tags) {
                userErrors { message }
              }
            }",
            new { id = orderId, tags = new[] { tag } },
            ct);

        // =====================================================
        // 📝 SİSTEM NOTU (SADECE ara1)
        // =====================================================
        if (decision.Decision == OrderDecision.ApprovalNeeded &&
            decision.Reasons.Any())
        {
            var systemNote =
                "[SİSTEM]\n" +
                string.Join("\n",
                    decision.Reasons
                        .Distinct()
                        .Select(r => $"• {r}"));

            var existingNote = order["note"]?.ToString();

            var finalNote = string.IsNullOrWhiteSpace(existingNote)
                ? systemNote
                : $"{systemNote}\n\n[MÜŞTERİ NOTU]\n{existingNote}";

            await _graphQl.ExecuteAsync(
                @"mutation ($id: ID!, $note: String!) {
                  orderUpdate(input: { id: $id, note: $note }) {
                    userErrors { message }
                  }
                }",
                new { id = orderId, note = finalNote },
                ct);
        }

        // =====================================================
        // 🔥 AYNI MÜŞTERİNİN DİĞER AÇIK SİPARİŞLERİNİ ara1’E ÇEK
        // (İLK SİPARİŞ DHL OLSA BİLE)
        // =====================================================
        if (decision.Decision == OrderDecision.ApprovalNeeded)
        {
            await ForceOtherOrdersToAra1Async(orderId, order, ct);
        }
    }

    // =====================================================
    // 🔥 AYNI TELEFONLU TÜM AÇIK SİPARİŞLERİ ara1 YAP
    // =====================================================
    private async Task ForceOtherOrdersToAra1Async(
        string currentOrderId,
        JObject order,
        CancellationToken ct)
    {
        var phone =
            order["shipping_address"]?["phone"]?.ToString();

        if (string.IsNullOrWhiteSpace(phone))
            return;

        var gql = @"
query ($phone: String!) {
  orders(
    first: 50
    query: ""financial_status:pending fulfillment_status:unfulfilled phone:$phone""
  ) {
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
            new { phone },
            ct);

        if (json?["data"]?["orders"]?["edges"] is not JArray edges)
            return;

        foreach (var edge in edges)
        {
            var node = edge?["node"] as JObject;
            var orderId = node?["id"]?.ToString();

            if (string.IsNullOrWhiteSpace(orderId))
                continue;

            // 🔒 Kendini tekrar işleme
            if (orderId == currentOrderId)
                continue;

            var existingTags =
                node["tags"]?.ToString()?.ToLowerInvariant() ?? string.Empty;

            // Zaten ara1 ise geç
            if (existingTags.Contains("ara1"))
                continue;

            // Önce eski tagleri sil
            var tagsToRemove = existingTags
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .ToArray();

            if (tagsToRemove.Length > 0)
            {
                await _graphQl.ExecuteAsync(
                    @"mutation ($id: ID!, $tags: [String!]!) {
                      tagsRemove(id: $id, tags: $tags) {
                        userErrors { message }
                      }
                    }",
                    new { id = orderId, tags = tagsToRemove },
                    ct);
            }

            // ara1 ekle
            await _graphQl.ExecuteAsync(
                @"mutation ($id: ID!, $tags: [String!]!) {
                  tagsAdd(id: $id, tags: $tags) {
                    userErrors { message }
                  }
                }",
                new { id = orderId, tags = new[] { "ara1" } },
                ct);
        }
    }
}
