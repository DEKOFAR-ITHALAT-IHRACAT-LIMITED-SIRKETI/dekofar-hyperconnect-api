using Newtonsoft.Json.Linq;
using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;

public class ShopifyOrderAutoTagService
{
    private readonly ShopifyOrderTagEngine _engine;
    private readonly ShopifyGraphQlClient _graphQl;

    public ShopifyOrderAutoTagService(
        ShopifyOrderTagEngine engine,
        ShopifyGraphQlClient graphQl)
    {
        _engine = engine;
        _graphQl = graphQl;
    }

    public async Task ApplyAutoTagsAsync(
        JObject order,
        CancellationToken ct)
    {
        var result =
            await _engine.CalculateAsync(order, ct);

        if (result == null)
            return;

        var orderId =
            order["admin_graphql_api_id"]?.ToString();

        if (string.IsNullOrWhiteSpace(orderId))
            return;

        await AddTagAsync(orderId, result.Tag, ct);

        await UpdateNoteAsync(
            orderId,
            $"[SİSTEM] {result.Reason}",
            order["note"]?.ToString(),
            ct);
    }

    private async Task AddTagAsync(
        string orderId,
        string tag,
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
            new { id = orderId, tags = new[] { tag } },
            ct);
    }

    private async Task UpdateNoteAsync(
        string orderId,
        string systemNote,
        string? customerNote,
        CancellationToken ct)
    {
        var finalNote = string.IsNullOrWhiteSpace(customerNote)
            ? systemNote
            : $"{systemNote}\n— Müşteri notu:\n{customerNote}";

        var mutation = @"
mutation ($id: ID!, $note: String!) {
  orderUpdate(input: { id: $id, note: $note }) {
    userErrors { message }
  }
}";

        await _graphQl.ExecuteAsync(
            mutation,
            new { id = orderId, note = finalNote },
            ct);
    }
}
