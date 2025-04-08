using Application.Interfaces;
using Core.Parameters;
using Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Web.Controllers
{
    [Authorize(Roles = "Customer")]
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService ticketService;
        private readonly ITicketValidationService ticketValidationService;

        public TicketController(ITicketService ticketService, ITicketValidationService ticketValidationService)
        {
            this.ticketService = ticketService;
            this.ticketValidationService = ticketValidationService;
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

        [HttpGet("can-purchase/{eventId}")]
        public async Task<ActionResult<bool>> CanPurchaseAsync(int eventId)
        {
            var result = await ticketValidationService.CanPurchaseTicketAsync(eventId);

            if (!result.IsValid)
                return BadRequest(result.Reason);

            return Ok(true);
        }
    }
}
