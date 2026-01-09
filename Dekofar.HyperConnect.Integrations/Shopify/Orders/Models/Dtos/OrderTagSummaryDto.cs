using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Models.Dtos
{
    public class OrderTagSummaryDto
    {
        public string Tag { get; set; } = default!;
        public int OrderCount { get; set; }
    }

}
