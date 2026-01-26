using Dekofar.HyperConnect.Application.Shipments.Commands;
using Dekofar.HyperConnect.Application.Shipments.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace dekofar_hyperconnect_api.Controllers.Shipments
{
    [ApiController]
    [Route("api/shipments")]
    public class ShipmentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ShipmentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateShipmentRequest request)
        {
            var result = await _mediator.Send(
                new CreateShipmentCommand(request));

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }

}
