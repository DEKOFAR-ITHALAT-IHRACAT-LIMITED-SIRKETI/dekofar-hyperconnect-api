using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Tracking.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace dekofar_hyperconnect_api.Controllers.Kargo.ptt
{
    [Route("api/kargo/ptt/tracking")]
    [ApiController]
    public class PttTrackingController : ControllerBase
    {
        private readonly IPttTrackingService _pttTracking;

        public PttTrackingController(IPttTrackingService pttTracking)
        {
            _pttTracking = pttTracking;
        }

        /// <summary>
        /// Barkod numarası ile gönderi takibi yapar.
        /// </summary>
        [HttpGet("{barcode}")]
        public async Task<IActionResult> GetByBarcode(string barcode)
        {
            var result = await _pttTracking.GetByBarcodeAsync(barcode);
            return Ok(result);
        }

        /// <summary>
        /// Müşteri referans numarası ile gönderi takibi yapar.
        /// </summary>
        [HttpGet("ref/{referenceNo}")]
        public async Task<IActionResult> GetByReference(string referenceNo)
        {
            var result = await _pttTracking.GetByReferenceAsync(referenceNo);
            return Ok(result);
        }

        /// <summary>
        /// Test için rastgele barkod üretir ve PTT Takip servisine sorgu atar.
        /// </summary>
        [HttpGet("test-random")]
        public async Task<IActionResult> TestRandom()
        {
            var randomBarcode = BarcodeGenerator.GenerateRandomInRange(
                279178450000, 279178459999
            );

            var result = await _pttTracking.GetByBarcodeAsync(randomBarcode);

            return Ok(new
            {
                Barcode = randomBarcode,
                Response = result
            });
        }

        /// <summary>
        /// Test için sabit barkod kökünden (279178450000) 13 haneli barkod üretir ve sorgu atar.
        /// </summary>
        [HttpGet("test-fixed")]
        public async Task<IActionResult> TestFixed()
        {
            var fixedBase = "279178450000"; // 12 haneli sabit kök
            var fixedBarcode = BarcodeGenerator.Generate(fixedBase);

            var result = await _pttTracking.GetByBarcodeAsync(fixedBarcode);

            return Ok(new
            {
                Barcode = fixedBarcode,
                Response = result
            });
        }
    }
}
