using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Models.Responses;

namespace Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Interfaces
{
    public interface IPttDeleteService
    {
        Task<PttDeleteResponse> DeleteByReferenceAsync(string dosyaAdi, string referansNo);
        Task<PttDeleteResponse> DeleteByBarcodeAsync(string dosyaAdi, string barcode);
    }
}
