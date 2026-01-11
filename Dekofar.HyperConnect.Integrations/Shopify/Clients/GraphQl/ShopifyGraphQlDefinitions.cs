using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.GraphQl
{
    internal static class ShopifyGraphQlQueries
    {
        public const string OpenOrdersMinimal = @"...";
        public const string OpenOrdersFull = @"...";
        public const string OpenOrdersByPhone = @"...";
    }

    internal static class ShopifyGraphQlMutations
    {
        public const string TagsRemove = @"...";
        public const string TagsAdd = @"...";
        public const string UpdateOrderNote = @"...";
    }
}

