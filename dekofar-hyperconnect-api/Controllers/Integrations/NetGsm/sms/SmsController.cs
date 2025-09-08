using Dekofar.HyperConnect.Integrations.NetGsm.Interfaces.sms;
using Dekofar.HyperConnect.Integrations.NetGsm.Models.sms;
using Microsoft.AspNetCore.Mvc;

namespace Dekofar.API.Controllers.Integrations
{
    [ApiController]
    [Route("api/[controller]")]
    public class SmsController : ControllerBase
    {
        private readonly INetGsmSmsService _smsService;

        public SmsController(INetGsmSmsService smsService)
        {
            _smsService = smsService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SmsSendRequest request)
        {
            var result = await _smsService.SendSmsAsync(request);
            return Ok(result);
        }
    }

}
