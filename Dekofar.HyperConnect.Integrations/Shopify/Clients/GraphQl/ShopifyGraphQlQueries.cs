namespace Dekofar.HyperConnect.Integrations.Shopify.GraphQl
{
    internal static class ShopifyGraphQlQueries
    {
        /// <summary>
        /// Açık siparişler – minimal (reset için)
        /// </summary>
        public const string OpenOrdersMinimal = @"
query ($cursor: String, $first: Int!) {
  orders(
    first: $first
    after: $cursor
    query: ""financial_status:pending fulfillment_status:unfulfilled""
  ) {
    pageInfo {
      hasNextPage
      endCursor
    }
    edges {
      node {
        id
        tags
      }
    }
  }
}";

        /// <summary>
        /// Aynı telefon numarasına sahip AÇIK siparişler
        /// ✔ SADECE ara1 zorlaması için
        /// ✔ Webhook + AutoTag kullanır
        /// </summary>
        public const string OpenOrdersByPhone = @"
query ($phone: String!) {
  orders(
    first: 50
    query: ""financial_status:pending fulfillment_status:unfulfilled phone:$phone""
  ) {
    edges {
      node {
        id
        tags
      }
    }
  }
}";
    }
}
