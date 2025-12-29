using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;

public class ShopifyOrderAutoTagService
{
    private static readonly string[] ManagedTags =
    {
        "ara1",
        "dhl",
        "ptt",
        "iptal"
    };

    private readonly ShopifyGraphQlClient _graphQl;
    private readonly ShopifyOrderTagEngine _tagEngine;

    public ShopifyOrderAutoTagService(
        ShopifyGraphQlClient graphQl,
        ShopifyOrderTagEngine tagEngine)
    {
        _graphQl = graphQl;
        _tagEngine = tagEngine;
    }

    public async Task ApplyAutoTagsAsync(
        JObject order,
        CancellationToken ct,
        bool replaceExistingTags = false)
    {
        var orderId =
            order["admin_graphql_api_id"]?.ToString();

        if (string.IsNullOrWhiteSpace(orderId))
            return;

        var result =
            await _tagEngine.CalculateAsync(order, ct);

        if (result == null)
            return;

        // 🧹 SADECE BİZİM ETİKETLERİ SİL
        if (replaceExistingTags)
        {
            var existingTags =
                order["tags"]?.ToString();

            if (!string.IsNullOrWhiteSpace(existingTags))
            {
                var tagsToRemove = existingTags
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => ManagedTags.Contains(t))
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
            }
        }

        // 🏷️ TEK ETİKET
        await _graphQl.ExecuteAsync(
            @"mutation ($id: ID!, $tags: [String!]!) {
                tagsAdd(id: $id, tags: $tags) {
                  userErrors { message }
                }
              }",
            new { id = orderId, tags = new[] { result.Tag } },
            ct);

        // 📝 SADECE ARA1 İSE SİSTEM NOTU
        if (result.Tag == "ara1" && result.Reasons.Any())
        {
            var systemNote =
                "[SİSTEM - ARA1]\n" +
                string.Join("\n", result.Reasons.Select(r => $"• {r}"));

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
