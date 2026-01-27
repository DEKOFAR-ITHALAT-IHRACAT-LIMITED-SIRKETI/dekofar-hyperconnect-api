using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Application.Shipments.Common
{
    public enum ShipmentStatus
    {
        Created = 0,
        Accepted = 1,
        InTransit = 2,
        Delivered = 3,
        Cancelled = 4
    }
}
