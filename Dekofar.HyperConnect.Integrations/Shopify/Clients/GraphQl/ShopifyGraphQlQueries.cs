namespace Dekofar.HyperConnect.Integrations.Shopify.GraphQl
{
    public static class ShopifyGraphQlQueries
    {
        public const string OpenOrdersMinimal = @"
query ($first: Int!, $cursor: String) {
  orders(
    first: $first
    after: $cursor
    query: ""financial_status:pending fulfillment_status:unfulfilled""
  ) {
    pageInfo { hasNextPage endCursor }
    edges {
      node {
        id
        tags
      }
    }
  }
}";

        public const string OpenOrdersFull = @"
query ($first: Int!, $cursor: String) {
  orders(
    first: $first
    after: $cursor
    query: ""financial_status:pending fulfillment_status:unfulfilled""
  ) {
    pageInfo { hasNextPage endCursor }
    edges {
      node {
        id
        note
        totalPriceSet { shopMoney { amount } }
        shippingAddress {
          address1
          city
          phone
          countryCode
        }
        customer {
          numberOfOrders
        }
        lineItems(first: 50) {
          edges {
            node {
              quantity
              product { id }
            }
          }
        }
      }
    }
  }
}";
    }
}
