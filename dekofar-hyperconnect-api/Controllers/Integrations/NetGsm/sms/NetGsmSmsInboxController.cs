using Dekofar.HyperConnect.Integrations.NetGsm.Interfaces;
using Dekofar.HyperConnect.Integrations.NetGsm.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace dekofar_hyperconnect_api.Controllers.Integrations.NetGsm.sms
{
    [ApiController]
    [Route("api/integrations/netgsm/sms")]
    public class NetGsmSmsInboxController : ControllerBase
    {
        private readonly INetGsmSmsInboxService _inboxService;

        public NetGsmSmsInboxController(INetGsmSmsInboxService inboxService)
        {
            _inboxService = inboxService;
        }

        /// <summary>
        /// Gelen SMS kutusunu listeler (Swagger test)
        /// </summary>
        [HttpPost("inbox")]
        public async Task<IActionResult> GetInbox([FromBody] SmsInboxRequest request)
        {
            var result = await _inboxService.GetInboxAsync(request);
            return Ok(result);
        }
    }
}
