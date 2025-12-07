using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace dekofar_hyperconnect_api.Controllers.Integrations.Meta
{
    [ApiController]
    [Route("api/meta/webhook")]
    public class MetaWebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MetaWebhookController> _logger;

        public MetaWebhookController(
            IConfiguration configuration,
            ILogger<MetaWebhookController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // GET: verification
        [HttpGet]
        public IActionResult Verify(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.verify_token")] string verifyToken,
            [FromQuery(Name = "hub.challenge")] string challenge)
        {
            var expected = _configuration["Meta:VerifyToken"];

            if (verifyToken == expected)
            {
                return Ok(challenge);
            }

            _logger.LogWarning("Webhook verification failed. Expected {Expected}, got {Got}", expected, verifyToken);
            return Unauthorized();
        }

        // POST: actual events (comments vs.)
        [HttpPost]
        public IActionResult Receive([FromBody] object body)
        {
            _logger.LogInformation("Meta webhook event: {Body}", body.ToString());
            // Burada ileride: yorumları parse + auto-reply kurgusunu kuracağız.
            return Ok("EVENT_RECEIVED");
        }
    }
}
