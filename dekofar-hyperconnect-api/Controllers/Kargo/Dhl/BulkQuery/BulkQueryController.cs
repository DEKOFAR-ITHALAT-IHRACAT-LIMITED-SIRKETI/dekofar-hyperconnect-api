using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Models;
using Microsoft.AspNetCore.Mvc;

namespace dekofar_hyperconnect_api.Controllers.Kargo.Dhl.BulkQuery
{
    [ApiController]
    [Route("api/kargo/dhl/bulkquery")]
    public class BulkQueryController : ControllerBase
    {
        private readonly IShipmentByDateService _shipmentByDateService; // DHL'e ait olabilir
        private readonly IDeliveredShipmentService _deliveredShipmentService; // DHL'e ait olabilir
        private readonly IStatusChangedShipmentService _statusChangedShipmentService;

        public BulkQueryController(
            IShipmentByDateService shipmentByDateService,
            IDeliveredShipmentService deliveredShipmentService,
            IStatusChangedShipmentService statusChangedShipmentService)
        {
            _shipmentByDateService = shipmentByDateService;
            _deliveredShipmentService = deliveredShipmentService;
            _statusChangedShipmentService = statusChangedShipmentService;
        }

        /// <summary>
        /// Belirtilen tarihteki gönderileri getirir.
        /// </summary>
        [HttpGet("shipments/bydate/{startDate}")]
        [ProducesResponseType(typeof(List<ShipmentByDateResponse>), 200)]
        public async Task<IActionResult> GetShipmentsByDate(string startDate)
        {
            var result = await _shipmentByDateService.GetShipmentByDateAsync(startDate);
            return Ok(result);
        }

        /// <summary>
        /// Belirtilen tarihte teslim edilmiş gönderileri getirir.
        /// </summary>
        [HttpGet("shipments/delivered/{startDate}")]
        [ProducesResponseType(typeof(List<DeliveredShipmentResponse>), 200)]
        public async Task<IActionResult> GetDeliveredShipments(string startDate)
        {
            var result = await _deliveredShipmentService.GetDeliveredShipmentsAsync(startDate);
            return Ok(result);
        }

        /// <summary>
        /// Belirtilen tarih aralığında hareket gören MNG gönderilerini getirir.
        /// </summary>
        /// <param name="startDate">dd.MM.yyyy</param>
        /// <param name="endDate">dd.MM.yyyy HH:mm:ss</param>
        [HttpGet("shipments/status-changed/{startDate}/{endDate}")]
        [ProducesResponseType(typeof(List<StatusChangedShipmentResponse>), 200)]
        public async Task<IActionResult> GetStatusChangedShipments(string startDate, string endDate)
        {
            var result = await _statusChangedShipmentService.GetStatusChangedShipmentsAsync(startDate, endDate);
            return Ok(result);
        }


    }
}
