using Dekofar.HyperConnect.Application.Shipments.Commands;
using Dekofar.HyperConnect.Application.Shipments.Commands.Create;
using Dekofar.HyperConnect.Application.Shipments.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dekofar_hyperconnect_api.Controllers.Shipments;

[ApiController]
[Route("api/[controller]")]
public class ShipmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShipmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// PTT kargo oluşturur
    /// </summary>
    [HttpPost]
    [AllowAnonymous] // 🔑 TESTTE ŞART
    public async Task<IActionResult> Create(
        [FromBody] CreateShipmentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(
            new CreateShipmentCommand(request));

        if (!result.Success)
            return BadRequest(result);   // ❗ 400 = iş kuralı / PTT hatası

        return Ok(result);               // ✅ 200 = başarılı
    }
}
