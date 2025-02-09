using Application.Interfaces;
using Core.Parameters;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
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
        public async Task<IActionResult> CreateDummy([FromBody] PaymentParams paymentParams)
        {
            var response = await paymentService.ProcessAsync(paymentParams, 100, "ticket");
            return Ok(response);
        }
    }
}
