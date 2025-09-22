using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Ptt.Shipment.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace dekofar_hyperconnect_api.Controllers.Kargo.ptt
{
    [ApiController]
    [Route("api/kargo/ptt/shipment")]
    public class PttShipmentController : ControllerBase
    {
        private readonly IPttShipmentService _shipmentService;

        public PttShipmentController(IPttShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] List<PttShipmentRequest> requests)
        {
            var result = await _shipmentService.AddShipmentsAsync(requests);
            return Ok(result);
        }
    }

}
