using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Core.Parameters;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Concertable.Web.Controllers;

[Authorize(Roles = "Customer")]
[ApiController]
[Route("api/[controller]")]
public class TicketController : ControllerBase
{
    private readonly ITicketService ticketService;
    private readonly ITicketValidator ticketValidator;

    public TicketController(ITicketService ticketService, ITicketValidator ticketValidator)
    {
        this.ticketService = ticketService;
        this.ticketValidator = ticketValidator;
    }

    [HttpPost("purchase")]
    public async Task<ActionResult<TicketPurchaseDto>> Purchase([FromBody] TicketPurchaseParams purchaseParams)
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
        var result = await ticketValidator.CanPurchaseTicketAsync(eventId);

        if (!result.IsValid)
            return BadRequest(result.Errors);

        return Ok(true);
    }
}
