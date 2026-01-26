using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Application.Shipments
{
    public class CreateShipmentResult
    {
        public bool Success { get; set; }
        public string? TrackingNo { get; set; }
        public string? Error { get; set; }
    }

}
