using System.Collections.Generic;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Models
{
    public class GetOrderResponse
    {
        public Order order { get; set; }
        public List<ShipmentPiece> orderPieceList { get; set; }  // ✅ ortak ShipmentPiece
        public StandardQueryShipper shipper { get; set; }
        public StandardQueryRecipient recipient { get; set; }
    }

    public class Order
    {
        public string orderType { get; set; }
        public string shipmentServiceType { get; set; }
        public string packagingType { get; set; }
        public int? isTransformedToShipment { get; set; }
        public string paymentType { get; set; }
        public string deliveryType { get; set; }
        public string orderDate { get; set; }
        public int? isVerifiedOnMarketPlace { get; set; }
        public string referenceId { get; set; }
        public string barcode { get; set; }
        public string billOfLandingId { get; set; }
        public int? isCOD { get; set; }
        public decimal? codAmount { get; set; }
        public string content { get; set; }
        public int? smsPreference1 { get; set; }
        public int? smsPreference2 { get; set; }
        public int? smsPreference3 { get; set; }
        public string description { get; set; }
        public string marketPlaceShortCode { get; set; }
        public string marketPlaceSaleCode { get; set; }
    }
}
