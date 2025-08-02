using System.Threading.Tasks;
using Dekofar.HyperConnect.Application.ResponseTemplates.Commands;
using Dekofar.HyperConnect.Application.ResponseTemplates.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dekofar.API.Controllers
{
    [ApiController]
    [Route("api/response-templates")]
    [Authorize]
    public class ResponseTemplatesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ResponseTemplatesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? module)
        {
            var templates = await _mediator.Send(new GetResponseTemplatesQuery(module));
            return Ok(templates);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateResponseTemplateCommand command)
        {
            if (command.IsGlobal && !User.IsInRole("Admin"))
                return Forbid();

            var id = await _mediator.Send(command);
            return Ok(id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateResponseTemplateCommand command)
        {
            if (id != command.Id) return BadRequest();

            var existing = await _mediator.Send(new GetResponseTemplateByIdQuery(id));
            if (existing == null) return NotFound();
            if ((existing.IsGlobal || command.IsGlobal) && !User.IsInRole("Admin"))
                return Forbid();

            await _mediator.Send(command);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _mediator.Send(new GetResponseTemplateByIdQuery(id));
            if (existing == null) return NotFound();
            if (existing.IsGlobal && !User.IsInRole("Admin"))
                return Forbid();

            await _mediator.Send(new DeleteResponseTemplateCommand(id));
            return Ok();
        }
    }
}
