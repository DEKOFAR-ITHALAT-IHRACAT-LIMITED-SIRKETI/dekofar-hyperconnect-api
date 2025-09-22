using System.Collections.Generic;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Models
{
    // Gönderi parçaları
    public class ShipmentPiece
    {
        public int? numberOfPieces { get; set; }
        public int? kgDesi { get; set; }
        public string barcode { get; set; }
        public int? desi { get; set; }
        public int? kg { get; set; }
        public string content { get; set; }
    }

    // Gönderici bilgisi
    public class StandardQueryShipper
    {
        public long? customerId { get; set; }   // ✅ null destekli
        public string refCustomerId { get; set; }
        public string city { get; set; }
        public string district { get; set; }
        public string cityName { get; set; }
        public string districtName { get; set; }
        public string address { get; set; }
        public string bussinessPhoneNumber { get; set; }
        public string email { get; set; }
        public string taxOffice { get; set; }
        public string taxNumber { get; set; }
        public string fullName { get; set; }
        public string homePhoneNumber { get; set; }
        public string mobilePhoneNumber { get; set; }
    }

    // Alıcı bilgisi
    public class StandardQueryRecipient
    {
        public long? customerId { get; set; }
        public string refCustomerId { get; set; }
        public string city { get; set; }
        public string district { get; set; }
        public string cityName { get; set; }
        public string districtName { get; set; }
        public string address { get; set; }
        public string bussinessPhoneNumber { get; set; }
        public string email { get; set; }
        public string taxOffice { get; set; }
        public string taxNumber { get; set; }
        public string fullName { get; set; }
        public string homePhoneNumber { get; set; }
        public string mobilePhoneNumber { get; set; }
    }
}
