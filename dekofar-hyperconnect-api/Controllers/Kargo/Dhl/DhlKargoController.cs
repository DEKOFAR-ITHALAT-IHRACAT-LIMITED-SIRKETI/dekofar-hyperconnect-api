using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace dekofar_hyperconnect_api.Controllers.Kargo.Dhl
{
    [ApiController]
    [Route("api/[controller]")]
    public class DhlKargoController : ControllerBase
    {
        private readonly IDhlKargoAuthService _authService;
        private readonly IDhlKargoShipmentService _shipmentService;

        public DhlKargoController(
            IDhlKargoAuthService authService,
            IDhlKargoShipmentService shipmentService)
        {
            _authService = authService;
            _shipmentService = shipmentService;
        }

        /// <summary>
        /// DHL Kargo için JWT token alır.
        /// </summary>
        [HttpGet("token")]
        public async Task<IActionResult> GetToken()
        {
            var tokenResponse = await _authService.GetTokenAsync();
            return Ok(tokenResponse);
        }

        /// <summary>
        /// ShipmentId’ye göre ilk statüyü getirir.
        /// </summary>
        [HttpGet("shipment-status/{shipmentId}")]
        public async Task<IActionResult> GetShipmentStatus(string shipmentId)
        {
            var status = await _shipmentService.GetShipmentStatusByShipmentIdAsync(shipmentId);
            return Ok(status);
        }

        /// <summary>
        /// ShipmentId’ye göre tüm statü geçmişini getirir.
        /// </summary>
        [HttpGet("shipment-status-list/{shipmentId}")]
        public async Task<IActionResult> GetShipmentStatusList(string shipmentId)
        {
            var statusList = await _shipmentService.GetShipmentStatusListByShipmentIdAsync(shipmentId);
            return Ok(statusList);
        }

        /// <summary>
        /// ShipmentId’ye göre tüm hareket geçmişini getirir.
        /// </summary>
        [HttpGet("track-shipment/{shipmentId}")]
        public async Task<IActionResult> TrackShipment(string shipmentId)
        {
            var movements = await _shipmentService.TrackShipmentByShipmentIdAsync(shipmentId);
            return Ok(movements);
        }

        /// <summary>
        /// ShipmentId’ye göre detaylı gönderi bilgisini getirir (shipment info + parçalar + gönderici + alıcı).
        /// </summary>
        [HttpGet("shipment/{shipmentId}")]
        public async Task<IActionResult> GetShipment(string shipmentId)
        {
            var detail = await _shipmentService.GetShipmentByShipmentIdAsync(shipmentId);
            return Ok(detail);
        }
    }
}
