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

    /// <summary>
    /// Siparişi kurallara göre yeniden etiketler
    /// replaceExistingTags = true → eski etiketleri tamamen siler
    /// </summary>
    public async Task ApplyAutoTagsAsync(
        JObject order,
        CancellationToken ct,
        bool replaceExistingTags = false)
    {
        var orderId =
            order["admin_graphql_api_id"]?.ToString();

        if (string.IsNullOrWhiteSpace(orderId))
            return;

        // 🧠 KURALLARI ÇALIŞTIR (TEK SONUÇ)
        var result =
            await _tagEngine.CalculateAsync(order, ct);

        if (result == null)
            return;

        // 🔥 ESKİ ETİKETLERİ SİL
        if (replaceExistingTags)
        {
            var clearTagsMutation = @"
mutation ($id: ID!) {
  tagsReplace(id: $id, tags: []) {
    userErrors { message }
  }
}";
            await _graphQl.ExecuteAsync(
                clearTagsMutation,
                new { id = orderId },
                ct);
        }

        // 🏷️ YENİ TEK ETİKET
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
