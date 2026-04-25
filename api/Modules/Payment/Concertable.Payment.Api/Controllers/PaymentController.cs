using Concertable.Payment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Payment.Api.Controllers;

[ApiController]
[Route("api/payment")]
internal class PaymentController : ControllerBase
{
    private readonly IPaymentService paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        this.paymentService = paymentService;
    }

    [HttpPost("dummy")]
    public Task<IActionResult> CreateDummy([FromBody] string transactionId)
    {
        return Task.FromResult<IActionResult>(Ok());
    }
}
