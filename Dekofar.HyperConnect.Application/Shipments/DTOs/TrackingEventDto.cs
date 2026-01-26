using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Application.Shipments.DTOs
{
    public class TrackingEventDto
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = default!;
        public string? Location { get; set; }
    }
}
