using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Infrastructure.Services;
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
        private readonly IDhlKargoDeliveredShipmentService _deliveredService;

        public DhlKargoController(
            IDhlKargoAuthService authService,
            IDhlKargoShipmentService shipmentService,
            IDhlKargoDeliveredShipmentService deliveredService)
        {
            _authService = authService;
            _shipmentService = shipmentService;
            _deliveredService = deliveredService;
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


        /// <summary>
        /// Belirtilen tarihte teslim edilen tüm gönderileri getirir.
        /// </summary>
        /// <param name="date">Format: yyyy-MM-dd</param>
        [HttpGet("delivered-shipments")]
        public async Task<IActionResult> GetDeliveredShipments([FromQuery] DateTime date)
        {
            if (date == default)
                return BadRequest("Geçerli bir tarih giriniz. Örn: 2025-08-26");

            var result = await _deliveredService.GetDeliveredShipmentsByDateAsync(date);
            return Ok(result);
        }

        /// <summary>
        /// Son N gün içinde teslim edilen gönderileri getirir.
        /// </summary>
        /// <param name="days">Kaç gün geriye bakılacak (default: 7)</param>
        [HttpGet("delivered-shipments/range")]
        public async Task<IActionResult> GetDeliveredShipmentsRange([FromQuery] int days = 7)
        {
            if (days <= 0)
                return BadRequest("Gün sayısı 0'dan büyük olmalı.");

            var results = new List<object>();

            for (int i = 0; i < days; i++)
            {
                var date = DateTime.Today.AddDays(-i);
                var shipments = await _deliveredService.GetDeliveredShipmentsByDateAsync(date);

                results.Add(new
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Count = shipments.Count,
                    Shipments = shipments
                });
            }

            return Ok(results);
        }
        [HttpGet("job-stats/today")]
        public async Task<IActionResult> GetTodayJobStats(
            [FromServices] IJobStatsService statsService,
            CancellationToken ct = default)
        {
            var stat = await statsService.GetTodayStatsAsync(ct);
            if (stat == null)
                return Ok(new { Date = DateTime.Today.ToString("yyyy-MM-dd"), PaidMarked = 0, CancelTagged = 0 });

            return Ok(stat);
        }

        [HttpGet("job-stats/history")]
        public async Task<IActionResult> GetJobStatsHistory(
            [FromServices] IJobStatsService statsService,
            CancellationToken ct,
            [FromQuery] int days = 30)   // opsiyonel en sonda ✅
        {
            var history = await statsService.GetStatsHistoryAsync(days, ct);
            return Ok(history);
        }





    }
}
