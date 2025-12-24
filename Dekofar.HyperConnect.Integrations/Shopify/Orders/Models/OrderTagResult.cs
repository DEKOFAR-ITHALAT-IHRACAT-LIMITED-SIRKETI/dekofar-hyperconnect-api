using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Models
{
    public class OrderTagResult
    {
        public string Tag { get; set; } = default!;
        public string? Reason { get; set; }
    }
}
