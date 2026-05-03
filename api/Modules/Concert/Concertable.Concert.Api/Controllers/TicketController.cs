using Concertable.Concert.Api.Requests;
using Concertable.Concert.Application.DTOs;
using Concertable.Concert.Application.Interfaces;
using Concertable.Payment.Application.Responses;
using Concertable.Payment.Domain;
using Concertable.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Concert.Api.Controllers;

[Authorize(Roles = "Customer")]
[ApiController]
[Route("api/[controller]")]
internal class TicketController : ControllerBase
{
    private readonly ITicketService ticketService;
    private readonly ITicketValidator ticketValidator;

    public TicketController(ITicketService ticketService, ITicketValidator ticketValidator)
    {
        this.ticketService = ticketService;
        this.ticketValidator = ticketValidator;
    }

    [HttpPost("purchase")]
    public async Task<ActionResult<TicketPaymentResponse>> Purchase([FromBody] TicketPurchaseParams purchaseParams)
    {
        var result = await ticketService.PurchaseAsync(purchaseParams);

        if (result.IsFailed)
            return BadRequest(result.Errors.SelectMessages());

        return Ok(result.Value);
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<TicketCheckout>> Checkout([FromBody] TicketCheckoutRequest request)
    {
        var result = await ticketService.CheckoutAsync(request.ConcertId);

        if (result.IsFailed)
            return BadRequest(result.Errors.SelectMessages());

        return Ok(result.Value);
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

    [HttpGet("concert/{concertId}/eligibility")]
    public async Task<ActionResult<bool>> CanPurchase(int concertId)
    {
        var result = await ticketValidator.CanBePurchasedAsync(concertId);
        return Ok(result.IsSuccess);
    }
}
