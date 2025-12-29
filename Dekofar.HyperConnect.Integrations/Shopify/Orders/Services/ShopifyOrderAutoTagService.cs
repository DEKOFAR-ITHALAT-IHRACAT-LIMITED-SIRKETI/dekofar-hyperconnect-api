using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;

public class ShopifyOrderAutoTagService
{
    private readonly ShopifyGraphQlClient _graphQl;
    private readonly ShopifyOrderTagEngine _engine;

    public ShopifyOrderAutoTagService(
        ShopifyGraphQlClient graphQl,
        ShopifyOrderTagEngine engine)
    {
        _graphQl = graphQl;
        _engine = engine;
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

        var result =
            await _engine.CalculateAsync(order, ct);

        if (result == null)
            return;

        // 🧹 Eski etiketleri sil
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

        // 🏷️ Tek etiket
        await _graphQl.ExecuteAsync(
            @"mutation ($id: ID!, $tags: [String!]!) {
                tagsAdd(id: $id, tags: $tags) {
                  userErrors { message }
                }
              }",
            new { id = orderId, tags = new[] { result.Tag } },
            ct);

        // 📝 Sistem notu
        if (result.Notes.Any())
        {
            var systemNote =
                "[SİSTEM]\n" +
                string.Join("\n", result.Notes.Select(n => $"• {n}"));

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
