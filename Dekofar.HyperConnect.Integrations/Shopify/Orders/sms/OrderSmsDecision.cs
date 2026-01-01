using Dekofar.HyperConnect.Integrations.Shopify.Orders.Decisions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.sms
{
    public class OrderSmsDecision
    {
        public string Phone { get; set; } = default!;
        public OrderDecision Decision { get; set; }
        public string? ShippingCarrier { get; set; }
    }
}
