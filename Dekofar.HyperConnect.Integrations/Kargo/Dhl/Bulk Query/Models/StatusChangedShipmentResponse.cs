using System.Text.Json.Serialization;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Models
{
    /// <summary>
    /// MNG getStatusChangedShipments yanıt modeli
    /// </summary>
    public class StatusChangedShipmentResponse
    {
        [JsonPropertyName("orderId")]
        public string OrderId { get; set; }

        [JsonPropertyName("referenceId")]
        public string ReferenceId { get; set; }

        [JsonPropertyName("shipmentId")]
        public string ShipmentId { get; set; }

        [JsonPropertyName("shipmentSerialandNumber")]
        public string ShipmentSerialandNumber { get; set; }

        [JsonPropertyName("shipmentDateTime")]
        public string ShipmentDateTime { get; set; }

        [JsonPropertyName("shipmentStatus")]
        public string ShipmentStatus { get; set; }

        [JsonPropertyName("shipmentStatusCode")]
        public int ShipmentStatusCode { get; set; }

        [JsonPropertyName("shipmentStatusExplanation")]
        public string ShipmentStatusExplanation { get; set; }

        [JsonPropertyName("statusDateTime")]
        public string StatusDateTime { get; set; }

        [JsonPropertyName("trackingUrl")]
        public string TrackingUrl { get; set; }

        [JsonPropertyName("isDelivered")]
        public int IsDelivered { get; set; }

        [JsonPropertyName("deliveryDateTime")]
        public string DeliveryDateTime { get; set; }

        [JsonPropertyName("deliveryTo")]
        public string DeliveryTo { get; set; }

        [JsonPropertyName("retrieveShipmentId")]
        public string RetrieveShipmentId { get; set; }
    }
}
