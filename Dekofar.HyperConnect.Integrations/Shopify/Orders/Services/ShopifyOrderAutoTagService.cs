using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Models;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;

/// <summary>
/// Shopify siparişlerine otomatik etiket uygulayan servis
/// </summary>
public class ShopifyOrderAutoTagService
{
    private readonly ShopifyGraphQlClient _graphQl;
    private readonly ShopifyOrderTagEngine _engine;

    // Sistem tarafından yönetilen etiketler
    private static readonly string[] ManagedTags =
    {
        "ara1",
        "dhl",
        "ptt",
        "iptal",
        "onay",
        "bekle"
    };

    public ShopifyOrderAutoTagService(
        ShopifyGraphQlClient graphQl,
        ShopifyOrderTagEngine engine)
    {
        _graphQl = graphQl;
        _engine = engine;
    }

    /// <summary>
    /// Siparişi analiz eder, eski otomatik etiketleri siler
    /// ve kurallara göre TEK etiket uygular
    /// </summary>
    public async Task ApplyAutoTagsAsync(
        JObject order,
        CancellationToken ct)
    {
        var orderId =
            order["admin_graphql_api_id"]?.ToString();

        if (string.IsNullOrWhiteSpace(orderId))
            return;

        // 🔍 Kuralları çalıştır (tek sonuç döner)
        var result = await _engine.CalculateAsync(order, ct);

        if (result == null)
            return;

        var mutation = @"
mutation (
  $id: ID!,
  $removeTags: [String!]!,
  $addTags: [String!]!,
  $note: String
) {
  tagsRemove(id: $id, tags: $removeTags) {
    userErrors { message }
  }
  tagsAdd(id: $id, tags: $addTags) {
    userErrors { message }
  }
  orderUpdate(input: { id: $id, note: $note }) {
    userErrors { message }
  }
}";

        await _graphQl.ExecuteAsync(
            mutation,
            new
            {
                id = orderId,
                removeTags = ManagedTags,
                addTags = new[] { result.Tag },
                note = result.Reason
            },
            ct);
    }
}
