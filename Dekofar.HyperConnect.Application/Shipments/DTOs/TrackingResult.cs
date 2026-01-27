using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Application.Shipments.DTOs
{
    public class TrackingResult
    {
        public bool Success { get; set; }
        public string? Status { get; set; }
        public string? Error { get; set; }
    }
}
