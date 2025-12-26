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

        // 🧠 KURALLARI ÇALIŞTIR
        var result =
            await _tagEngine.CalculateAsync(order, ct);

        if (result == null)
            return;

        // 🧹 ESKİ ETİKETLERİ SİL (TEK TEK)
        if (replaceExistingTags)
        {
            var existingTags =
                order["tags"]?.ToString();

            if (!string.IsNullOrWhiteSpace(existingTags))
            {
                var tagsToRemove = existingTags
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .ToArray();

                if (tagsToRemove.Length > 0)
                {
                    var removeMutation = @"
mutation ($id: ID!, $tags: [String!]!) {
  tagsRemove(id: $id, tags: $tags) {
    userErrors { message }
  }
}";
                    await _graphQl.ExecuteAsync(
                        removeMutation,
                        new
                        {
                            id = orderId,
                            tags = tagsToRemove
                        },
                        ct);
                }
            }
        }

        // 🏷️ TEK ETİKET EKLE
        var addTagMutation = @"
mutation ($id: ID!, $tags: [String!]!) {
  tagsAdd(id: $id, tags: $tags) {
    userErrors { message }
  }
}";
        await _graphQl.ExecuteAsync(
            addTagMutation,
            new
            {
                id = orderId,
                tags = new[] { result.Tag }
            },
            ct);

        // 📝 NOT EKLE (MÜŞTERİ NOTUNU EZMEZ)
        if (!string.IsNullOrWhiteSpace(result.Note))
        {
            var existingNote =
                order["note"]?.ToString();

            var finalNote = string.IsNullOrWhiteSpace(existingNote)
                ? $"[SİSTEM] {result.Note}"
                : $"[SİSTEM] {result.Note}\n[MÜŞTERİ NOTU] {existingNote}";

            var noteMutation = @"
mutation ($id: ID!, $note: String!) {
  orderUpdate(input: { id: $id, note: $note }) {
    userErrors { message }
  }
}";
            await _graphQl.ExecuteAsync(
                noteMutation,
                new
                {
                    id = orderId,
                    note = finalNote
                },
                ct);
        }
    }
}
