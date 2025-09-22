using Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.StandardQuery.Models;
using Microsoft.AspNetCore.Mvc;

namespace dekofar_hyperconnect_api.Controllers.Kargo.Dhl.StandardQuery
{
    [ApiController]
    [Route("api/kargo/dhl/standardquery")]
    public class StandardQueryController : ControllerBase
    {
        private readonly IGetOrderService _getOrderService;
        private readonly IGetShipmentService _getShipmentService;
        private readonly IGetShipmentByShipmentIdService _getShipmentByShipmentIdService;
        private readonly IGetShipmentStatusByReferenceIdService _getShipmentStatusByReferenceIdService;
        private readonly IGetShipmentStatusByShipmentIdService _getShipmentStatusByShipmentIdService;
        private readonly ITrackShipmentByReferenceIdService _trackShipmentByReferenceIdService;
        private readonly ITrackShipmentByShipmentIdService _trackShipmentByShipmentIdService;

        public StandardQueryController(
            IGetOrderService getOrderService,
            IGetShipmentService getShipmentService,
            IGetShipmentByShipmentIdService getShipmentByShipmentIdService,
            IGetShipmentStatusByReferenceIdService getShipmentStatusByReferenceIdService,
            IGetShipmentStatusByShipmentIdService getShipmentStatusByShipmentIdService,
            ITrackShipmentByReferenceIdService trackShipmentByReferenceIdService,
            ITrackShipmentByShipmentIdService trackShipmentByShipmentIdService)
        {
            _getOrderService = getOrderService;
            _getShipmentService = getShipmentService;
            _getShipmentByShipmentIdService = getShipmentByShipmentIdService;
            _getShipmentStatusByReferenceIdService = getShipmentStatusByReferenceIdService;
            _getShipmentStatusByShipmentIdService = getShipmentStatusByShipmentIdService;
            _trackShipmentByReferenceIdService = trackShipmentByReferenceIdService;
            _trackShipmentByShipmentIdService = trackShipmentByShipmentIdService;
        }

        // 1) Get Order
        [HttpGet("order/{referenceId}")]
        [ProducesResponseType(typeof(GetOrderResponse), 200)]
        public async Task<IActionResult> GetOrder(string referenceId)
        {
            var result = await _getOrderService.GetOrderAsync(referenceId);
            return Ok(result);
        }

        // 2) Get Shipment
        [HttpGet("shipment/by-reference/{referenceId}")]
        [ProducesResponseType(typeof(GetShipmentResponse), 200)]
        public async Task<IActionResult> GetShipment(string referenceId)
        {
            var result = await _getShipmentService.GetShipmentAsync(referenceId);
            return Ok(result);
        }

        // 3) Get Shipment By ShipmentId
        [HttpGet("shipment/by-shipmentid/{shipmentId}")]
        [ProducesResponseType(typeof(List<GetShipmentResponse>), 200)]
        public async Task<IActionResult> GetShipmentByShipmentId(string shipmentId)
        {
            var result = await _getShipmentByShipmentIdService.GetShipmentByShipmentIdAsync(shipmentId);
            return Ok(result);
        }



        // 4) Get Shipment Status By ReferenceId
        [HttpGet("shipment-status/by-reference/{referenceId}")]
        [ProducesResponseType(typeof(GetShipmentStatusResponse), 200)]
        public async Task<IActionResult> GetShipmentStatusByReferenceId(string referenceId)
        {
            var result = await _getShipmentStatusByReferenceIdService.GetShipmentStatusByReferenceIdAsync(referenceId);
            return Ok(result);
        }

        // 5) Get Shipment Status By ShipmentId
        [HttpGet("shipment-status/by-shipmentid/{shipmentId}")]
        [ProducesResponseType(typeof(GetShipmentStatusResponse), 200)]
        public async Task<IActionResult> GetShipmentStatusByShipmentId(string shipmentId)
        {
            var result = await _getShipmentStatusByShipmentIdService.GetShipmentStatusByShipmentIdAsync(shipmentId);
            return Ok(result);
        }

        // 6) Track Shipment By ReferenceId
        [HttpGet("track/by-reference/{referenceId}")]
        [ProducesResponseType(typeof(List<TrackShipmentResponse>), 200)]
        public async Task<IActionResult> TrackShipmentByReferenceId(string referenceId)
        {
            var result = await _trackShipmentByReferenceIdService.TrackShipmentByReferenceIdAsync(referenceId);
            return Ok(result);
        }

        // 7) Track Shipment By ShipmentId
        [HttpGet("track/by-shipmentid/{shipmentId}")]
        [ProducesResponseType(typeof(List<TrackShipmentResponse>), 200)]
        public async Task<IActionResult> TrackShipmentByShipmentId(string shipmentId)
        {
            var result = await _trackShipmentByShipmentIdService.TrackShipmentByShipmentIdAsync(shipmentId);
            return Ok(result);
        }
    }
}
