using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Models;

namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Interfaces
{
    /// <summary>
    /// Belirtilen tarihteki gönderileri getirir (DHL/MNG ortak endpoint).
    /// </summary>
    public interface IShipmentByDateDetailService
    {
        /// <summary>
        /// İlgili tarihteki gönderileri listeler.
        /// </summary>
        /// <param name="date">Tarih (dd.MM.yyyy)</param>
        Task<List<ShipmentByDateDetailResponse>> GetShipmentsByDateDetailAsync(string date);
    }
}
