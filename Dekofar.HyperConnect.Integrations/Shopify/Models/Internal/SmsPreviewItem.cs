using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Shopify.Models.Internal
{
    public class SmsPreviewItem
    {
        public long OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public string Phone { get; set; } = null!;
        public string Carrier { get; set; } = null!;
        public string TrackingNumber { get; set; } = null!;
        public string? TrackingUrl { get; set; }
        public string Message { get; set; } = null!;
    }
}
