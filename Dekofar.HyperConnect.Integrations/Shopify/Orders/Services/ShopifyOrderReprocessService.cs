using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Newtonsoft.Json.Linq;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services;

public class ShopifyOrderReprocessService
{
    private readonly ShopifyGraphQlClient _graphQl;
    private readonly ShopifyOrderAutoTagService _autoTag;

    // 🔧 BATCH AYARLARI
    private const int BatchSize = 200;
    private static readonly TimeSpan BatchDelay = TimeSpan.FromMinutes(2);

    public ShopifyOrderReprocessService(
        ShopifyGraphQlClient graphQl,
        ShopifyOrderAutoTagService autoTag)
    {
        _graphQl = graphQl;
        _autoTag = autoTag;
    }

    // =====================================================
    // 🚀 TEK AKIŞ
    // 1️⃣ Etiket + sistem notlarını temizle
    // 2️⃣ Bekle
    // 3️⃣ 200’er 200’er yeniden etiketle
    // =====================================================
    public async Task<int> ReprocessOpenOrdersAsync(CancellationToken ct)
    {
        await ClearSystemTagsAndNotesAsync(ct);

        // Shopify eventual consistency için
        await Task.Delay(TimeSpan.FromMinutes(1), ct);

        return await ReprocessInternalAsync(ct);
    }

    // =====================================================
    // 🔁 ETİKETLEME (INTERNAL)
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
    pageInfo {{
      hasNextPage
      endCursor
    }}
    edges {{
      node {{
        id
        tags
        note
        totalPriceSet {{ shopMoney {{ amount }} }}
        shippingAddress {{
          address1
          city
          phone
          countryCode
        }}
        customer {{ numberOfOrders }}
        lineItems(first: 50) {{
          edges {{
            node {{
              product {{ id }}
            }}
          }}
        }}
      }}
    }}
  }}
}}";

            var json = await _graphQl.ExecuteAsync(gql, new { cursor }, ct);
            var ordersObj = json["data"]?["orders"] as JObject;
            if (ordersObj == null) break;

            hasNextPage =
                ordersObj["pageInfo"]?["hasNextPage"]?.Value<bool>() == true;

            cursor =
                ordersObj["pageInfo"]?["endCursor"]?.ToString();

            var edges = ordersObj["edges"] as JArray;
            if (edges == null || edges.Count == 0) continue;

            // 🔑 TELEFON SAYIMI (BATCH İÇİ)
            var phoneCounts = new Dictionary<string, int>();

            foreach (var edge in edges)
            {
                var phone =
                    edge?["node"]?["shippingAddress"]?["phone"]?.ToString();

                if (string.IsNullOrWhiteSpace(phone)) continue;

                phoneCounts.TryGetValue(phone, out var c);
                phoneCounts[phone] = c + 1;
            }

            foreach (var edge in edges)
            {
                if (edge?["node"] is not JObject node) continue;

                var normalized =
                    NormalizeGraphQlOrder(node, phoneCounts);

                await _autoTag.ApplyAutoTagsAsync(
                    normalized,
                    ct,
                    replaceExistingTags: true);

                processed++;
            }

            if (hasNextPage)
                await Task.Delay(BatchDelay, ct);
        }

        return processed;
    }

    // =====================================================
    // 🧹 TÜM ETİKETLER + SİSTEM NOTU TEMİZLE
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
    pageInfo {{
      hasNextPage
      endCursor
    }}
    edges {{
      node {{
        id
        tags
        note
      }}
    }}
  }}
}}";

            var json = await _graphQl.ExecuteAsync(gql, new { cursor }, ct);
            var ordersObj = json["data"]?["orders"] as JObject;
            if (ordersObj == null) break;

            hasNextPage =
                ordersObj["pageInfo"]?["hasNextPage"]?.Value<bool>() == true;

            cursor =
                ordersObj["pageInfo"]?["endCursor"]?.ToString();

            var edges = ordersObj["edges"] as JArray;
            if (edges == null || edges.Count == 0) continue;

            foreach (var edge in edges)
            {
                var node = edge?["node"] as JObject;
                var orderId = node?["id"]?.ToString();
                if (string.IsNullOrWhiteSpace(orderId)) continue;

                // 🏷️ TÜM ETİKETLER
                var tags = node["tags"]?.ToString()?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .ToArray();

                if (tags != null && tags.Length > 0)
                {
                    await _graphQl.ExecuteAsync(
                        @"mutation ($id: ID!, $tags: [String!]!) {
                          tagsRemove(id: $id, tags: $tags) {
                            userErrors { message }
                          }
                        }",
                        new { id = orderId, tags },
                        ct);
                }

                // 📝 SİSTEM NOTU
                var note = node["note"]?.ToString();
                if (!string.IsNullOrWhiteSpace(note) &&
                    note.StartsWith("[SİSTEM]"))
                {
                    var cleaned = RemoveSystemNote(note);

                    await _graphQl.ExecuteAsync(
                        @"mutation ($id: ID!, $note: String!) {
                          orderUpdate(input: { id: $id, note: $note }) {
                            userErrors { message }
                          }
                        }",
                        new { id = orderId, note = cleaned },
                        ct);
                }
            }

            if (hasNextPage)
                await Task.Delay(BatchDelay, ct);
        }
    }

    // =====================================================
    // 🔄 SAFE NORMALIZE
    // =====================================================
    private static JObject NormalizeGraphQlOrder(
        JObject node,
        Dictionary<string, int> phoneCounts)
    {
        var shipping = node["shippingAddress"] as JObject;
        var customer = node["customer"] as JObject;

        var phone = shipping?["phone"]?.ToString();
        phoneCounts.TryGetValue(phone ?? "", out var repeatCount);

        var lineItems = new JArray();
        var edges = node["lineItems"]?["edges"] as JArray;

        if (edges != null)
        {
            foreach (var e in edges)
            {
                var productId =
                    e?["node"]?["product"]?["id"]?.ToString();

                if (!string.IsNullOrWhiteSpace(productId))
                {
                    lineItems.Add(new JObject
                    {
                        ["product_id"] = productId
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
            ["__repeat_phone_count"] = repeatCount
        };
    }

    private static string RemoveSystemNote(string note)
    {
        var index = note.IndexOf("[MÜŞTERİ NOTU]");
        return index >= 0 ? note.Substring(index) : string.Empty;
    }
}
