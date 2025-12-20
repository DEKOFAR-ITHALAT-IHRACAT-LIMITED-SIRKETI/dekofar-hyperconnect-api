using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Tracking.Models.Responses;

namespace Dekofar.HyperConnect.Integrations.Kargo.Ptt.Tracking.Interfaces
{
    public interface IPttTrackingService
    {
        Task<PttTrackingResponse> GetByBarcodeAsync(string barcode);
        Task<PttTrackingResponse> GetByReferenceAsync(string referenceNo);
    }
}
