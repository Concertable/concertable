using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Parameters;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        this.paymentService = paymentService;
    }

    [HttpPost("dummy")]
    public Task<IActionResult> CreateDummy([FromBody] string transactionId)
    {
        //var response = await paymentService.ProcessAsync(paymentParams, 100, "ticket");
        return Task.FromResult<IActionResult>(Ok());
    }
}
