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
        [FromServices] IAcceptanceDispatcher acceptanceDispatcher)
    {
        var outcome = await acceptanceDispatcher.AcceptAsync(applicationId);
        return Ok(outcome);
    }

    [Authorize]
    [HttpPost("complete")]
    public async Task<IActionResult> Complete(
        [FromQuery] int concertId,
        [FromServices] ICompletionDispatcher completionDispatcher)
    {
        var result = await completionDispatcher.FinishAsync(concertId);

        if (result.IsFailed)
            return BadRequest(result.Errors.Select(e => e.Message));

        return Ok(result.Value);
    }
}
