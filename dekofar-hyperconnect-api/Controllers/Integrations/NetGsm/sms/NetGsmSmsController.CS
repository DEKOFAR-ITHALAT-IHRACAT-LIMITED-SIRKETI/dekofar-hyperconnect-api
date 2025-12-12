using Dekofar.HyperConnect.Integrations.NetGsm.Interfaces;
using Dekofar.HyperConnect.Integrations.NetGsm.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace dekofar_hyperconnect_api.Controllers.Integrations.NetGsm.sms
{
    [ApiController]
    [Route("api/integrations/netgsm/sms")]
    public class NetGsmSmsController : ControllerBase
    {
        private readonly INetGsmSmsSendService _smsSendService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NetGsmSmsController> _logger;

        public NetGsmSmsController(
            INetGsmSmsSendService smsSendService,
            IConfiguration configuration,
            ILogger<NetGsmSmsController> logger)
        {
            _smsSendService = smsSendService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Swagger test için SMS gönderir
        /// </summary>
        [HttpPost("send")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SmsSendResponse), 200)]
        public async Task<IActionResult> SendSms([FromBody] SmsSendRequest request)
        {
            if (request == null)
                return BadRequest("Request body boş olamaz.");

            if (request.Messages == null || request.Messages.Count == 0)
                return BadRequest("En az 1 mesaj gönderilmelidir.");

            if (string.IsNullOrWhiteSpace(request.MsgHeader))
            {
                request.MsgHeader = _configuration["NetGsm:DefaultHeader"];
            }

            _logger.LogInformation(
                "📨 SMS gönderim isteği alındı. Mesaj sayısı: {Count}",
                request.Messages.Count
            );

            var result = await _smsSendService.SendAsync(request);

            return Ok(result);
        }
    }
}
