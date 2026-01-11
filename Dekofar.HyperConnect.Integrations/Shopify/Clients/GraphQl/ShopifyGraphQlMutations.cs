using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.GraphQl
{
    public static class ShopifyGraphQlMutations
    {
        public const string TagsAdd = @"
mutation ($id: ID!, $tags: [String!]!) {
  tagsAdd(id: $id, tags: $tags) {
    userErrors { message }
  }
}";

        public const string TagsRemove = @"
mutation ($id: ID!, $tags: [String!]!) {
  tagsRemove(id: $id, tags: $tags) {
    userErrors { message }
  }
}";

        public const string UpdateOrderNote = @"
mutation ($id: ID!, $note: String!) {
  orderUpdate(input: { id: $id, note: $note }) {
    userErrors { message }
  }
}";
    }
}


