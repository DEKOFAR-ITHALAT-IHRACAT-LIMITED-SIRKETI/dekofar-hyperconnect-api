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
        var orderId =
            order["admin_graphql_api_id"]?.ToString();

        if (string.IsNullOrWhiteSpace(orderId))
            return;

        // =====================================================
        // 🧠 KARAR
        // =====================================================
        var decision = _decisionEngine.Decide(order);

        // =====================================================
        // 🏷️ KARAR → TAG
        // =====================================================
        var tag = decision.Decision switch
        {
            OrderDecision.Automatic => "dhl",
            OrderDecision.ApprovalNeeded => "ara1",
            OrderDecision.Cancelled => "IPTAL",
            _ => null
        };

        if (string.IsNullOrWhiteSpace(tag))
            return;

        // =====================================================
        // 🧹 TÜM ESKİ TAGLERİ SİL
        // =====================================================
        if (replaceExistingTags)
        {
            var existingTags =
                order["tags"]?.ToString();

            var tagsToRemove = existingTags?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToArray();

            if (tagsToRemove?.Length > 0)
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
        // 🏷️ YENİ TAG EKLE (TEK TAG)
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
        // 📝 SİSTEM NOTU (SADECE ARA1)
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

            var existingNote =
                order["note"]?.ToString();

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
    }
}
