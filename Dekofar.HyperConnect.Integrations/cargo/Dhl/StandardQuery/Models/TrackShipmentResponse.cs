namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Models
{
    public class TrackShipmentResponse
    {
        public string referenceId { get; set; }
        public string eventSequence { get; set; }
        public string eventStatus { get; set; }
        public string eventStatusEn { get; set; }
        public string eventDateTime { get; set; }
        public string eventDateTimeFormat { get; set; }
        public string eventDateTimezone { get; set; }
        public string eventDateTime2 { get; set; }
        public string eventDateTime2Format { get; set; }
        public string eventDateTime2zone { get; set; }
        public string location { get; set; }
        public string country { get; set; }
        public string locationAddress { get; set; }
        public string locationPhone { get; set; }
        public string deliveryDateTime { get; set; }
        public string deliveryTo { get; set; }
        public string description { get; set; }
        public string pieceTotal { get; set; }
    }
}
