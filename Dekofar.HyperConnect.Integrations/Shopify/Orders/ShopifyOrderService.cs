using Dekofar.HyperConnect.Integrations.Shopify.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders
{
    public class ShopifyOrderService
    {
        private readonly ShopifyRestClient _rest;

        public ShopifyOrderService(ShopifyRestClient rest)
        {
            _rest = rest;
        }

        public Task<bool> AddTagAsync(long orderId, string tag)
        {
            // REST → update order tags
            throw new NotImplementedException();
        }
    }

}
