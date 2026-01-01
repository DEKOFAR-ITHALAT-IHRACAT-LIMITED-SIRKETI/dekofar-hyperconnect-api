using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Decisions
{

    public class OrderDecisionResult
    {
        public OrderDecision Decision { get; set; }
        public List<string> Reasons { get; } = new();

        public bool RequiresSms =>
            Decision == OrderDecision.Automatic ||
            Decision == OrderDecision.ApprovalNeeded;
    }
}
