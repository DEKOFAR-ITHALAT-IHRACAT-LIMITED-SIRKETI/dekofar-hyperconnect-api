using Dekofar.HyperConnect.Integrations.Shopify.Clients.GraphQl;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Services
{
    /// <summary>
    /// Shopify siparişlerine otomatik etiket ekleme servisi
    /// </summary>
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
        /// Sipariş payload'ına göre otomatik etiketleri hesaplar
        /// ve Shopify siparişine ekler
        /// </summary>
        public async Task ApplyAutoTagsAsync(
            JObject order,
            CancellationToken ct)
        {
            var orderId =
                order["admin_graphql_api_id"]?.ToString();

            if (string.IsNullOrWhiteSpace(orderId))
                return;

            var tags =
                await _tagEngine.CalculateAsync(order, ct);

            if (!tags.Any())
                return;

            var mutation = @"
mutation ($id: ID!, $tags: [String!]!) {
  tagsAdd(id: $id, tags: $tags) {
    userErrors {
      field
      message
    }
  }
}";

            await _graphQl.ExecuteAsync(
                mutation,
                new
                {
                    id = orderId,
                    tags
                },
                ct);
        }
    }
}
