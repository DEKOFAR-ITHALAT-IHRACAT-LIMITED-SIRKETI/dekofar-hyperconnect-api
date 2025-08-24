using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.Interfaces
{
    public interface IDhlKargoShipmentService
    {
        Task<ShipmentStatusResponse> GetShipmentStatusByShipmentIdAsync(string shipmentId);
        Task<List<ShipmentStatusResponse>> GetShipmentStatusListByShipmentIdAsync(string shipmentId);
        Task<List<ShipmentTrackResponse>> TrackShipmentByShipmentIdAsync(string shipmentId);

        /// <summary>
        /// ShipmentId’ye göre gönderi detaylarını (shipment info + pieces + shipper + recipient) döner
        /// </summary>
        Task<ShipmentDetailResponse> GetShipmentByShipmentIdAsync(string shipmentId);
    }
}
