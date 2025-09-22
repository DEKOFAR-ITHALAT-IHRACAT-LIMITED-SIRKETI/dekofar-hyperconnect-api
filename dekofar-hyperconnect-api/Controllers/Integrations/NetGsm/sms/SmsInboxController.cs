using Dekofar.HyperConnect.Integrations.NetGsm.Interfaces.sms;
using Dekofar.HyperConnect.Integrations.NetGsm.Models.sms;
using Microsoft.AspNetCore.Mvc;

namespace dekofar_hyperconnect_api.Controllers.Integrations.NetGsm.sms
{
    [ApiController]
    [Route("api/[controller]")]
    public class SmsInboxController : ControllerBase
    {
        private readonly INetGsmSmsService _smsService;
        private readonly ILogger<SmsInboxController> _logger;

        public SmsInboxController(INetGsmSmsService smsService, ILogger<SmsInboxController> logger)
        {
            _smsService = smsService;
            _logger = logger;
        }

        /// <summary>
        /// NetGSM gelen SMS kutusunu getirir.
        /// </summary>
        [HttpPost("list")]
        public async Task<IActionResult> GetInbox([FromBody] SmsInboxRequest request)
        {
            try
            {
                var result = await _smsService.GetInboxMessagesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ SMS inbox çekilirken hata oluştu.");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
