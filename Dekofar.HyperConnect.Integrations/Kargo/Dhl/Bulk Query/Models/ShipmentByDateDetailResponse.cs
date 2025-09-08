namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Models
{
    public class ShipmentByDateDetailResponse
    {
        public string ShipmentDate { get; set; }
        public string ShipmentDateTime { get; set; }
        public string ShipmentSerialNumber { get; set; }
        public string ShipmentNumber { get; set; }
        public string ShipmentType { get; set; }
        public string PaymentType { get; set; }
        public int PieceCount { get; set; }
        public string ReceivingBranch { get; set; }
        public string ShipperBranch { get; set; }
        public string TotalKgDesi { get; set; }
        public string FinalTotal { get; set; }
        public string ReferenceId { get; set; }
        public string BillOfLandingId { get; set; }
        public string CodAmount { get; set; }
        public string IsCOD { get; set; }
        public string IsDelivered { get; set; }
        public string DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }
        public string DeliveryTo { get; set; }
        public string ShipmentStatus { get; set; }
        public string ShipmentStatusExplanation { get; set; }
        public string StatusDateTime { get; set; }
        public string TrackingUrl { get; set; }
        public string ScheduledPaymentDate { get; set; }
        public string ActualPaymentDate { get; set; }
        public string ActualCollectionType { get; set; }
        public Party Shipper { get; set; }
        public Party Recipient { get; set; }
    }

    public class Party
    {
        public long CustomerId { get; set; }
        public string RefCustomerId { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string CityName { get; set; }
        public string DistrictName { get; set; }
        public string Address { get; set; }
        public string BussinessPhoneNumber { get; set; }
        public string Email { get; set; }
        public string TaxOffice { get; set; }
        public string TaxNumber { get; set; }
        public string FullName { get; set; }
        public string HomePhoneNumber { get; set; }
        public string MobilePhoneNumber { get; set; }
    }
}
