using Application.Interfaces;
using Core.Parameters;
using Core.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService ticketService;

        public TicketController(ITicketService ticketService)
        {
            this.ticketService = ticketService;
        }

        [HttpPost("purchase")]
        public async Task<ActionResult<TicketPurchaseResponse>> Purchase([FromBody] TicketPurchaseParams purchaseParams)
        {
            return Ok(await ticketService.PurchaseAsync(purchaseParams.PaymentMethodId, purchaseParams.EventId));
        }
    }
}
