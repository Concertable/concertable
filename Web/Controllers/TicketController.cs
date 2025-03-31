using Application.Interfaces;
using Core.Parameters;
using Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;

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
            return Ok(await ticketService.PurchaseAsync(purchaseParams));
        }

        [HttpGet("upcoming/user")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetUserUpcoming()
        {
            return Ok(await ticketService.GetUserUpcomingAsync());
        }

        [HttpGet("history/user")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetUserHistory()
        {
            return Ok(await ticketService.GetUserHistoryAsync());
        }
    }
}
