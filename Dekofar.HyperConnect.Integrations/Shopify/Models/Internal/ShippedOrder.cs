using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Models.Internal
{
    
    public class ShippedOrder
{
    public long OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public string? Phone { get; set; }
    public List<ShippedTracking> Trackings { get; set; } = new();
}

public class ShippedTracking
{
    public string TrackingNumber { get; set; } = default!;
    public string? Company { get; set; }
    public string? TrackingUrl { get; set; }
}
}