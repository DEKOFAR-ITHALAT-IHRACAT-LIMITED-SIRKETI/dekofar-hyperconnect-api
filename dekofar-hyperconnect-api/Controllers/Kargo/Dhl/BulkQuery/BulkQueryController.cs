using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery.Models;
using Microsoft.AspNetCore.Mvc;

namespace dekofar_hyperconnect_api.Controllers.Kargo.Dhl.BulkQuery
{
    [ApiController]
    [Route("api/kargo/dhl/bulkquery")]
    public class BulkQueryController : ControllerBase
    {
        private readonly IShipmentByDateService _shipmentByDateService;
        private readonly IDeliveredShipmentService _deliveredShipmentService;
        private readonly IStatusChangedShipmentService _statusChangedShipmentService;
        private readonly IShipmentByDateDetailService _shipmentByDateDetailService;

        public BulkQueryController(
            IShipmentByDateService shipmentByDateService,
            IDeliveredShipmentService deliveredShipmentService,
            IStatusChangedShipmentService statusChangedShipmentService,
            IShipmentByDateDetailService shipmentByDateDetailService)
        {
            _shipmentByDateService = shipmentByDateService;
            _deliveredShipmentService = deliveredShipmentService;
            _statusChangedShipmentService = statusChangedShipmentService;
            _shipmentByDateDetailService = shipmentByDateDetailService;
        }

        [HttpGet("shipments/bydate/{startDate}")]
        [ProducesResponseType(typeof(List<ShipmentByDateResponse>), 200)]
        public async Task<IActionResult> GetShipmentsByDate(string startDate)
        {
            var result = await _shipmentByDateService.GetShipmentByDateAsync(startDate);
            return Ok(result);
        }

        [HttpGet("shipments/delivered/{startDate}")]
        [ProducesResponseType(typeof(List<DeliveredShipmentResponse>), 200)]
        public async Task<IActionResult> GetDeliveredShipments(string startDate)
        {
            var result = await _deliveredShipmentService.GetDeliveredShipmentsAsync(startDate);
            return Ok(result);
        }

        [HttpGet("shipments/status-changed/{startDate}/{endDate}")]
        [ProducesResponseType(typeof(List<StatusChangedShipmentResponse>), 200)]
        public async Task<IActionResult> GetStatusChangedShipments(string startDate, string endDate)
        {
            var result = await _statusChangedShipmentService.GetStatusChangedShipmentsAsync(startDate, endDate);
            return Ok(result);
        }

        [HttpPost("shipments/bydate-detail")]
        [Consumes("application/json-patch+json")]
        [ProducesResponseType(typeof(List<ShipmentByDateDetailResponse>), 200)]
        public async Task<IActionResult> GetShipmentsByDateDetail([FromBody] DateRequest request)
        {
            var result = await _shipmentByDateDetailService.GetShipmentsByDateDetailAsync(request.Date);
            return Ok(result);
        }



    }
    public class DateRequest
    {
        public string Date { get; set; } = string.Empty;   // 🔑 sadece tek tarih
        public int ReportType { get; set; } = 1;
        public int SubCompany { get; set; } = 1;
    }

}
