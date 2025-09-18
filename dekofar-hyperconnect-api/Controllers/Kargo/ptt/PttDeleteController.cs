using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace dekofar_hyperconnect_api.Controllers.Kargo.ptt
{
    [Route("api/kargo/ptt/delete")]
    [ApiController]
    public class PttDeleteController : ControllerBase
    {
        private readonly IPttDeleteService _deleteService;

        public PttDeleteController(IPttDeleteService deleteService)
        {
            _deleteService = deleteService;
        }

        /// <summary>
        /// Referans numarası ile gönderi silme (referansVeriSil)
        /// </summary>
        /// <param name="dosyaAdi">Dosya adı (kabulEkle2 çağrısında kullanılan)</param>
        /// <param name="referansNo">Müşteri referans numarası</param>
        [HttpDelete("reference")]
        [ProducesResponseType(typeof(PttDeleteResponse), 200)]
        public async Task<IActionResult> DeleteByReference([FromQuery] string dosyaAdi, [FromQuery] string referansNo)
        {
            var result = await _deleteService.DeleteByReferenceAsync(dosyaAdi, referansNo);
            return Ok(result);
        }

        /// <summary>
        /// Barkod numarası ile gönderi silme (barkodVeriSil)
        /// </summary>
        /// <param name="dosyaAdi">Dosya adı (kabulEkle2 çağrısında kullanılan)</param>
        /// <param name="barcode">PTT barkod numarası</param>
        [HttpDelete("barcode")]
        [ProducesResponseType(typeof(PttDeleteResponse), 200)]
        public async Task<IActionResult> DeleteByBarcode([FromQuery] string dosyaAdi, [FromQuery] string barcode)
        {
            var result = await _deleteService.DeleteByBarcodeAsync(dosyaAdi, barcode);
            return Ok(result);
        }
    }
}
