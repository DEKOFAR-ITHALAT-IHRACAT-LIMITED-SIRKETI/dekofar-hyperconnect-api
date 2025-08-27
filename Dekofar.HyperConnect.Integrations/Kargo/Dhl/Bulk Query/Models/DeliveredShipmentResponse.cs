using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Models
{
    /// <summary>
    /// getDeliveredShipment endpoint yanıt modeli.
    /// </summary>
    public class DeliveredShipmentResponse
    {
        public Shipment Shipment { get; set; }
        public List<ShipmentPiece> ShipmentPieceList { get; set; }
        public Shipper Shipper { get; set; }
        public Recipient Recipient { get; set; }
    }
}
