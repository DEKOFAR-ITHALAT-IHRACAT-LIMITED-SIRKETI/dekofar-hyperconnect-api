using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Models
{
    /// <summary>
    /// getShipmentByDate endpoint yanıt modeli.
    /// </summary>
    public class ShipmentByDateResponse
    {
        public Shipment Shipment { get; set; }
        public List<ShipmentPiece> ShipmentPieceList { get; set; }
        public Shipper Shipper { get; set; }
        public Recipient Recipient { get; set; }
    }

    public class Shipment
    {
        public string ShipmentServiceType { get; set; }
        public string PackagingType { get; set; }
        public string PaymentType { get; set; }
        public string DeliveryType { get; set; }
        public string ReferenceId { get; set; }
        public string ShipmentId { get; set; }
        public string ShipmentSerialNumber { get; set; }
        public string ShipmentNumber { get; set; }
        public string ShipmentDateTime { get; set; }
        public int? PieceCount { get; set; }
        public decimal? TotalKg { get; set; }
        public decimal? TotalDesi { get; set; }
        public decimal? TotalKgDesi { get; set; }
        public decimal? Total { get; set; }
        public decimal? Kdv { get; set; }
        public decimal? FinalTotal { get; set; }
        public int? ShipmentStatusCode { get; set; }
        public int? IsMarketPlaceShipment { get; set; }
        public int? IsMarketPlacePays { get; set; }
        public string ShipperBranch { get; set; }
        public string BillOfLandingId { get; set; }
        public int? IsCOD { get; set; }
        public string CodAmount { get; set; }
        public string Content { get; set; }
        public string EstimatedDeliveryDate { get; set; }
        public int? IsDelivered { get; set; }
    }

    public class ShipmentPiece
    {
        public int? NumberOfPieces { get; set; }
        public int? KgDesi { get; set; }
        public string Barcode { get; set; }
        public int? Desi { get; set; }
        public int? Kg { get; set; }
        public string Content { get; set; }
    }

    public class Shipper
    {
        public int? CustomerId { get; set; }
        public string RefCustomerId { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Address { get; set; }
        public string BussinessPhoneNumber { get; set; }
        public string Email { get; set; }
        public string TaxOffice { get; set; }
        public string TaxNumber { get; set; }
        public string FullName { get; set; }
        public string HomePhoneNumber { get; set; }
        public string MobilePhoneNumber { get; set; }
    }

    public class Recipient
    {
        public string RefCustomerId { get; set; }
        public string City { get; set; }
        public string District { get; set; }
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

