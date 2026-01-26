using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Application.Shipments.DTOs
{
    public class CreateShipmentRequest
    {
        public string OrderId { get; set; } = default!;
        public string ReferenceId { get; set; } = default!;

        public string ReceiverName { get; set; } = default!;
        public string ReceiverPhone { get; set; } = default!;
        public string ReceiverAddress { get; set; } = default!;
        public string ReceiverCity { get; set; } = default!;
        public string ReceiverDistrict { get; set; } = default!;

        public bool IsCashOnDelivery { get; set; }
        public decimal? CashOnDeliveryAmount { get; set; }
    }

}
