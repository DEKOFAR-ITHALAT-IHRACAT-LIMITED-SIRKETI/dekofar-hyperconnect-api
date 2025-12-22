using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Infrastructure.Jobs;
using Dekofar.HyperConnect.Infrastructure.Services;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.Auth.Interfaces;
using Dekofar.HyperConnect.Integrations.Kargo.Dhl.BulkQuery;
using Microsoft.AspNetCore.Mvc;

namespace dekofar_hyperconnect_api.Controllers.Kargo.Dhl
{
    [ApiController]
    [Route("api/[controller]")]
    public class DhlKargoController : ControllerBase
    {
        private readonly IAuthService _authService;                 // ✅ DHL Auth
        private readonly IDeliveredShipmentService _deliveredService; // ✅ Delivered Shipments
        private readonly IRecurringJob _job;                         // ✅ DHL→Shopify Sync Job

        public DhlKargoController(
            IAuthService authService,
            IDeliveredShipmentService deliveredService,
            IRecurringJob job
        )
        {
            _authService = authService;
            _deliveredService = deliveredService;
            _job = job;
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
        /// Bugüne ait job istatistiklerini döndürür (kaç sipariş paid / cancel yapıldı).
        /// </summary>
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

        /// <summary>
        /// Job istatistik geçmişi (varsayılan son 30 gün).
        /// </summary>
        [HttpGet("job-stats/history")]
        public async Task<IActionResult> GetJobStatsHistory(
            [FromServices] IJobStatsService statsService,
            CancellationToken ct,
            [FromQuery] int days = 30)
        {
            var history = await statsService.GetStatsHistoryAsync(days, ct);
            return Ok(history);
        }

        /// <summary>
        /// DHL → Shopify senkron job’unu manuel tetikler.
        /// </summary>
        [HttpPost("sync-now")]
        public async Task<IActionResult> SyncNow(CancellationToken ct)
        {
            await _job.RunAsync(ct);
            return Ok(new { message = "✅ DHL → Shopify senkron çalıştırıldı." });
        }


    }
}
