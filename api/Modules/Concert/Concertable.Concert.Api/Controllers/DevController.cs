using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Concert.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
internal class DevController : ControllerBase
{
    [Authorize]
    [HttpPost("accept")]
    public async Task<IActionResult> Accept(
        [FromQuery] int applicationId,
        [FromServices] IAcceptDispatcher acceptDispatcher)
    {
        var outcome = await acceptDispatcher.AcceptAsync(applicationId);
        return Ok(outcome);
    }

    [Authorize]
    [HttpPost("complete")]
    public async Task<IActionResult> Complete(
        [FromQuery] int concertId,
        [FromServices] IFinishedDispatcher finishedDispatcher)
    {
        var result = await finishedDispatcher.FinishedAsync(concertId);

        if (result.IsFailed)
            return BadRequest(result.Errors.Select(e => e.Message));

        return Ok(result.Value);
    }
}
