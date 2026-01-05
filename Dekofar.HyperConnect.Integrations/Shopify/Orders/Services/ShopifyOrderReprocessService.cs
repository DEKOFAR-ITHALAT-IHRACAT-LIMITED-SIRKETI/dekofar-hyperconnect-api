using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;

public class ShopifyOrderReprocessService
{
    private readonly ShopifyGraphQlClient _graphQl;
    private readonly ShopifyOrderAutoTagService _autoTag;

    private const int BatchSize = 200;
    private static readonly TimeSpan BatchDelay = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan ConsistencyDelay = TimeSpan.FromMinutes(1);

    public ShopifyOrderReprocessService(
        ShopifyGraphQlClient graphQl,
        ShopifyOrderAutoTagService autoTag)
    {
        _graphQl = graphQl;
        _autoTag = autoTag;
    }

    // =====================================================
    // 🚀 ENTRY
    // =====================================================
    public async Task<int> ReprocessOpenOrdersAsync(CancellationToken ct)
    {
        await ClearSystemTagsAndNotesAsync(ct);

        // Shopify eventual consistency
        await Task.Delay(ConsistencyDelay, ct);

        return await ReprocessInternalAsync(ct);
    }

    // =====================================================
    // 🔁 MAIN LOOP (JVALUE SAFE)
    // =====================================================
    private async Task<int> ReprocessInternalAsync(CancellationToken ct)
    {
        string? cursor = null;
        bool hasNextPage = true;
        int processed = 0;

        while (hasNextPage)
        {
            ct.ThrowIfCancellationRequested();

            var gql = $@"
query ($cursor: String) {{
  orders(
    first: {BatchSize}
    after: $cursor
    query: ""financial_status:pending fulfillment_status:unfulfilled""
  ) {{
    pageInfo {{ hasNextPage endCursor }}
    edges {{
      node {{
        id
        tags
        note
        totalPriceSet {{ shopMoney {{ amount }} }}
        shippingAddress {{ address1 city phone countryCode }}
        customer {{ numberOfOrders }}
        lineItems(first: 50) {{
          edges {{
            node {{
              quantity
              product {{ id }}
            }}
          }}
        }}
      }}
    }}
  }}
}}";

            var json = await _graphQl.ExecuteAsync(gql, new { cursor }, ct);

            if (json?["data"]?["orders"] is not JObject ordersObj)
                break;

            if (ordersObj["pageInfo"] is not JObject pageInfo)
                break;

            hasNextPage = pageInfo["hasNextPage"]?.Value<bool>() == true;
            cursor = pageInfo["endCursor"]?.ToString();

            if (ordersObj["edges"] is not JArray edges || edges.Count == 0)
            {
                if (hasNextPage)
                    await Task.Delay(BatchDelay, ct);
                continue;
            }

            // =================================================
            // 📞 PHONE COUNT (BATCH İÇİ)
            // =================================================
            var phoneCounts = new Dictionary<string, int>();

            foreach (var edgeToken in edges)
            {
                if (edgeToken is not JObject edgeObj)
                    continue;

                if (edgeObj["node"] is not JObject node)
                    continue;

                if (node["shippingAddress"] is not JObject shipping)
                    continue;

                var phone = shipping["phone"]?.ToString();
                if (string.IsNullOrWhiteSpace(phone))
                    continue;

                phoneCounts.TryGetValue(phone, out var c);
                phoneCounts[phone] = c + 1;
            }

            // =================================================
            // 🏷️ TAG APPLY
            // =================================================
            foreach (var edgeToken in edges)
            {
                if (edgeToken is not JObject edgeObj)
                    continue;

                if (edgeObj["node"] is not JObject node)
                    continue;

                try
                {
                    var normalized =
                        NormalizeGraphQlOrder(node, phoneCounts);

                    await _autoTag.ApplyAutoTagsAsync(
                        normalized,
                        ct,
                        replaceExistingTags: true);

                    processed++;
                }
                catch
                {
                    // ❗ Tek sipariş bozuksa batch devam eder
                    continue;
                }
            }

            if (hasNextPage)
                await Task.Delay(BatchDelay, ct);
        }

        return processed;
    }

    // =====================================================
    // 🧹 CLEAN: TÜM TAGLER + SİSTEM NOTU
    // =====================================================
    private async Task ClearSystemTagsAndNotesAsync(CancellationToken ct)
    {
        string? cursor = null;
        bool hasNextPage = true;

        while (hasNextPage)
        {
            ct.ThrowIfCancellationRequested();

            var gql = $@"
query ($cursor: String) {{
  orders(
    first: {BatchSize}
    after: $cursor
    query: ""financial_status:pending fulfillment_status:unfulfilled""
  ) {{
    pageInfo {{ hasNextPage endCursor }}
    edges {{ node {{ id tags note }} }}
  }}
}}";

            var json = await _graphQl.ExecuteAsync(gql, new { cursor }, ct);

            if (json?["data"]?["orders"] is not JObject ordersObj)
                break;

            var pageInfo = ordersObj["pageInfo"] as JObject;
            hasNextPage = pageInfo?["hasNextPage"]?.Value<bool>() == true;
            cursor = pageInfo?["endCursor"]?.ToString();

            if (ordersObj["edges"] is not JArray edges)
                continue;

            foreach (var edgeToken in edges)
            {
                if (edgeToken is not JObject edgeObj)
                    continue;

                if (edgeObj["node"] is not JObject node)
                    continue;

                var id = node["id"]?.ToString();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                // 🏷️ TÜM TAGLER
                var tags = node["tags"]?.ToString()?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToArray();

                if (tags is { Length: > 0 })
                {
                    await _graphQl.ExecuteAsync(
                        @"mutation ($id: ID!, $tags: [String!]!) {
                          tagsRemove(id: $id, tags: $tags) {
                            userErrors { message }
                          }
                        }",
                        new { id, tags },
                        ct);
                }

                // 📝 SADECE [SİSTEM] BLOĞU
                var note = node["note"]?.ToString();
                if (!string.IsNullOrWhiteSpace(note) &&
                    note.StartsWith("[SİSTEM]"))
                {
                    await _graphQl.ExecuteAsync(
                        @"mutation ($id: ID!, $note: String!) {
                          orderUpdate(input: { id: $id, note: $note }) {
                            userErrors { message }
                          }
                        }",
                        new { id, note = RemoveSystemNote(note) },
                        ct);
                }
            }

            if (hasNextPage)
                await Task.Delay(BatchDelay, ct);
        }
    }

    // =====================================================
    // 🔄 NORMALIZE (product_id + quantity DAHİL)
    // =====================================================
    private static JObject NormalizeGraphQlOrder(
        JObject node,
        Dictionary<string, int> phoneCounts)
    {
        var shipping = node["shippingAddress"] as JObject;
        var customer = node["customer"] as JObject;

        var phone = shipping?["phone"]?.ToString();
        phoneCounts.TryGetValue(phone ?? "", out var repeat);

        var lineItems = new JArray();

        if (node["lineItems"] is JObject li &&
            li["edges"] is JArray edges)
        {
            foreach (var e in edges)
            {
                if (e?["node"] is not JObject item)
                    continue;

                var productId =
                    item["product"]?["id"]?.ToString();

                var quantity =
                    item["quantity"]?.Value<int>() ?? 1;

                if (!string.IsNullOrWhiteSpace(productId))
                {
                    lineItems.Add(new JObject
                    {
                        ["product_id"] = productId,
                        ["quantity"] = quantity
                    });
                }
            }
        }

        return new JObject
        {
            ["admin_graphql_api_id"] = node["id"]?.ToString(),
            ["tags"] = node["tags"]?.ToString(),
            ["note"] = node["note"]?.ToString(),

            ["total_price"] =
                node["totalPriceSet"]?["shopMoney"]?["amount"]?.ToString(),

            ["shipping_address"] = new JObject
            {
                ["address1"] = shipping?["address1"]?.ToString(),
                ["city"] = shipping?["city"]?.ToString(),
                ["phone"] = phone,
                ["country_code"] = shipping?["countryCode"]?.ToString()
            },

            ["customer"] = new JObject
            {
                ["orders_count"] =
                    customer?["numberOfOrders"]?.Value<int>() ?? 0
            },

            ["line_items"] = lineItems,
            ["__repeat_phone_count"] = repeat
        };
    }

    private static string RemoveSystemNote(string note)
    {
        var i = note.IndexOf("[MÜŞTERİ NOTU]");
        return i >= 0 ? note.Substring(i) : string.Empty;
    }
}
