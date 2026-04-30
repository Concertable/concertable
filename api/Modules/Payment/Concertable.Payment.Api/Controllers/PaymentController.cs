using Microsoft.AspNetCore.Mvc;

namespace Concertable.Payment.Api.Controllers;

[ApiController]
[Route("api/payment")]
internal class PaymentController : ControllerBase
{
    [HttpPost("dummy")]
    public Task<IActionResult> CreateDummy([FromBody] string transactionId)
    {
        return Task.FromResult<IActionResult>(Ok());
    }
}
