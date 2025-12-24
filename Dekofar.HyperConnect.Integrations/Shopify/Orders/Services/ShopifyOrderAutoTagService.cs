using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
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
        CancellationToken ct)
    {
        var orderId = order["admin_graphql_api_id"]?.ToString();
        if (string.IsNullOrWhiteSpace(orderId))
            return;

        var result = await _tagEngine.CalculateAsync(order, ct);
        if (result == null)
            return;

        // ✍️ TAG + NOTE MUTATION
        var mutation = @"
mutation ($id: ID!, $tags: [String!]!, $note: String!) {
  orderUpdate(
    input: {
      id: $id
      tags: $tags
      note: $note
    }
  ) {
    userErrors {
      field
      message
    }
  }
}";

        var note =
            result.Tag == "iptal" && !string.IsNullOrWhiteSpace(result.Reason)
                ? $"Otomatik iptal:\n{result.Reason}"
                : "";

        await _graphQl.ExecuteAsync(
            mutation,
            new
            {
                id = orderId,
                tags = new[] { result.Tag },
                note
            },
            ct);
    }
}
