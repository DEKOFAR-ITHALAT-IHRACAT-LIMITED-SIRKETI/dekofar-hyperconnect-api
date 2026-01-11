namespace Dekofar.HyperConnect.Integrations.Shopify.GraphQl
{
    public static class ShopifyGraphQlQueries
    {
        // =====================================================
        // 🔍 AÇIK SİPARİŞLER – SADECE ID + TAG
        // =====================================================
        public const string OpenOrdersMinimal = @"
query ($cursor: String, $first: Int!) {
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

        // =====================================================
        // 🔍 AÇIK SİPARİŞLER – TÜM KARAR DATASI
        // =====================================================
        public const string OpenOrdersFull = @"
query ($cursor: String, $first: Int!) {
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
        // =====================================================
        // 📞 AYNI TELEFONLU AÇIK SİPARİŞLER
        // =====================================================
        public const string OpenOrdersByPhone = @"
query ($phone: String!) {
  orders(
    first: 10
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
