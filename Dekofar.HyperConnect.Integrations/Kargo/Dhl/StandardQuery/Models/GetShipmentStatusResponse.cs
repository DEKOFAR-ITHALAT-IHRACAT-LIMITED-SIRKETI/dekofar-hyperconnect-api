namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Models
{
    public class GetShipmentStatusResponse
    {
        public int shipmentStatusCode { get; set; }
        public string orderId { get; set; }
        public string referenceId { get; set; }
        public string shipmentId { get; set; }
        public string shipmentSerialandNumber { get; set; }
        public string shipmentDateTime { get; set; }
        public string shipmentStatus { get; set; }
        public string statusDateTime { get; set; }
        public string trackingUrl { get; set; }
        public int isDelivered { get; set; }
        public string deliveryDateTime { get; set; }
        public string deliveryTo { get; set; }
    }
}
