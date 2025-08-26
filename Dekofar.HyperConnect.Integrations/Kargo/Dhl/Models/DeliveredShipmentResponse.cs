using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Services;
using System.Collections.Generic;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.Models
{
    public class DeliveredShipmentResponse
    {
        public ShipmentInfo? shipment { get; set; }
        public List<ShipmentPiece>? shipmentPieceList { get; set; }
        public Shipper? shipper { get; set; }
        public Recipient? recipient { get; set; }
    }
}
