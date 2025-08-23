using Dekofar.HyperConnect.Integrations.DHLKargo.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace dekofar_hyperconnect_api.Controllers.Kargo.Dhl
{
    [ApiController]
    [Route("api/[controller]")]
    public class DhlKargoController : ControllerBase
    {
        private readonly IDhlKargoAuthService _authService;

        public DhlKargoController(IDhlKargoAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("token")]
        public async Task<IActionResult> GetToken()
        {
            var tokenResponse = await _authService.GetTokenAsync();
            return Ok(tokenResponse); // JWT + refreshToken + expire tarihleri döner
        }
    }
}
