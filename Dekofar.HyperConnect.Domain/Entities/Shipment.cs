using Dekofar.HyperConnect.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Domain.Entities
{
    public enum ShipmentStatus
    {
        Created,
        SentToProvider,
        Accepted,
        InTransit,
        Delivered,
        Cancelled,
        Failed
    }

    public class Shipment : BaseEntity
    {
        public string OrderId { get; set; } = default!;
        public string Provider { get; set; } = "PTT";
        public string ReferenceId { get; set; } = default!;
        public string? TrackingNo { get; set; }

        public bool IsCashOnDelivery { get; set; }
        public decimal? CashOnDeliveryAmount { get; set; }

        public ShipmentStatus Status { get; set; }
        public string? LastError { get; set; }
    }

}
