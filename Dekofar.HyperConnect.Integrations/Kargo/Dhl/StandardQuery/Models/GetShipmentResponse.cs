using System.Collections.Generic;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Models
{
    public class GetShipmentResponse
    {
        public Shipment shipment { get; set; }
        public List<ShipmentPiece> shipmentPieceList { get; set; }
        public StandardQueryShipper shipper { get; set; }
        public StandardQueryRecipient recipient { get; set; }
    }

    public class Shipment
    {
        public string shipmentServiceType { get; set; }
        public string packagingType { get; set; }
        public string paymentType { get; set; }
        public string deliveryType { get; set; }
        public string referenceId { get; set; }
        public string shipmentId { get; set; }
        public string shipmentSerialNumber { get; set; }
        public string shipmentNumber { get; set; }
        public string shipmentDateTime { get; set; }
        public int? pieceCount { get; set; }
        public decimal? totalKg { get; set; }
        public decimal? totalDesi { get; set; }
        public decimal? totalKgDesi { get; set; }
        public decimal? total { get; set; }
        public decimal? kdv { get; set; }
        public decimal? finalTotal { get; set; }
        public int? shipmentStatusCode { get; set; }
        public int? isMarketPlaceShipment { get; set; }
        public int? isMarketPlacePays { get; set; }
        public string receivingBranch { get; set; }
        public string shipperBranch { get; set; }
        public string description { get; set; }
        public string billOfLandingId { get; set; }
        public int? isCOD { get; set; }
        public string codAmount { get; set; }
        public string content { get; set; }
        public string estimatedDeliveryDate { get; set; }
        public string deliveryDate { get; set; }
        public int? isDelivered { get; set; }
        public string deliveryTo { get; set; }
    }
}
