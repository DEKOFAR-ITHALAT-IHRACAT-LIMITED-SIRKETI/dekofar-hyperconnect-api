using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;

public class ShopifyOrderAutoTagService
{
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

        // 🧹 Eski etiketleri sil
        if (replaceExistingTags)
        {
            var existingTags =
                order["tags"]?.ToString();

            if (!string.IsNullOrWhiteSpace(existingTags))
            {
                var tags = existingTags
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .ToArray();

                if (tags.Length > 0)
                {
                    var removeMutation = @"
mutation ($id: ID!, $tags: [String!]!) {
  tagsRemove(id: $id, tags: $tags) {
    userErrors { message }
  }
}";
                    await _graphQl.ExecuteAsync(
                        removeMutation,
                        new { id = orderId, tags },
                        ct);
                }
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

        // 📝 Sistem Notu
        if (result.Reasons.Any())
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
