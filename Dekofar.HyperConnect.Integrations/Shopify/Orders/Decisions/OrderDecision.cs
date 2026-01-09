using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Decisions
{
    public enum OrderDecision
    {
        Automatic,      // Direkt onay (dhl)
        ApprovalNeeded, // ara1
        Cancelled       // iptal
    }
}

