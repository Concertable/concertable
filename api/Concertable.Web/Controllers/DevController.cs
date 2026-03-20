using Application.Interfaces.Concert;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevController : ControllerBase
{
    [HttpPost("accept")]
    public async Task<IActionResult> Accept(
        [FromQuery] int applicationId,
        [FromServices] IAcceptProcessor acceptProcessor)
    {
        await acceptProcessor.AcceptAsync(applicationId);
        return Ok();
    }

    [HttpPost("settle")]
    public async Task<IActionResult> Settle(
        [FromQuery] int concertId,
        [FromServices] ISettlementProcessor settlementProcessor)
    {
        await settlementProcessor.SettleAsync(concertId);
        return Ok();
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete(
        [FromQuery] int concertId,
        [FromServices] ICompleteProcessor completeProcessor)
    {
        await completeProcessor.CompleteAsync(concertId);
        return Ok();
    }
}
