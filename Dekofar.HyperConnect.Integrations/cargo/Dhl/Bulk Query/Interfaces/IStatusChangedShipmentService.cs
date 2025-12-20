namespace Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery
{
    using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Models;

    /// <summary>
    /// Belirtilen tarih aralığında hareket görmüş gönderileri getirir (MNG).
    /// </summary>
    public interface IStatusChangedShipmentService
    {
        /// <summary>
        /// İlgili tarih aralığında hareket gören gönderileri listeler.
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi (dd.MM.yyyy)</param>
        /// <param name="endDate">Bitiş tarihi (dd.MM.yyyy HH:mm:ss)</param>
        Task<List<StatusChangedShipmentResponse>> GetStatusChangedShipmentsAsync(string startDate, string endDate);
    }
}
