using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Entities;
using Concertable.Core.Entities.Contracts;
using Concertable.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevController : ControllerBase
{
    //[HttpGet("user")]
    //public async Task<ActionResult<IUser>> GetUser(
    //    [FromQuery] Guid id,
    //    [FromServices] IUserService userService)
    //{
    //    var user = await userService.GetUserByIdAsync(id);
    //    if (user is null) return NotFound();
    //    return Ok(user);
    //}

    //[HttpGet("debug")]
    //public async Task<IActionResult> Debug(
    //    [FromQuery] int applicationId,
    //    [FromServices] ApplicationDbContext context)
    //{
    //    var app = await context.OpportunityApplications
    //        .Where(a => a.Id == applicationId)
    //        .Select(a => new { a.Id, a.OpportunityId, a.Status })
    //        .FirstOrDefaultAsync();

    //    ContractEntity? contract = app != null
    //        ? await context.Contracts.Where(c => c.Id == app.OpportunityId).FirstOrDefaultAsync()
    //        : null;

    //    return Ok(new { app, contractExists = contract != null, contractType = contract?.ContractType.ToString() });
    //}

    //[HttpPost("seed-stripe")]
    //public async Task<IActionResult> SeedStripe(
    //    [FromServices] ApplicationDbContext context,
    //    [FromServices] IStripeAccountService stripeAccountService)
    //{
    //    var results = new List<object>();

    //    var customers = await context.Users.OfType<CustomerEntity>().ToListAsync();
    //    foreach (var customer in customers)
    //    {
    //        customer.StripeCustomerId = await stripeAccountService.CreateCustomerAsync(customer);
    //        results.Add(new { customer.Id, customer.Email, customer.StripeCustomerId });
    //    }

    //    var managers = await context.Users.OfType<ManagerEntity>().ToListAsync();
    //    foreach (var manager in managers)
    //    {
    //        manager.StripeCustomerId = await stripeAccountService.CreateCustomerAsync(manager);
    //        manager.StripeAccountId = await stripeAccountService.CreateConnectAccountAsync(manager);
    //        results.Add(new { manager.Id, manager.Email, manager.StripeCustomerId, manager.StripeAccountId });
    //    }

    //    await context.SaveChangesAsync();
    //    return Ok(results);
    //}

    [Authorize]
    [HttpPost("accept")]
    public async Task<IActionResult> Accept(
        [FromQuery] int applicationId,
        [FromServices] IAcceptDispatcher acceptDispatcher)
    {
        var outcome = await acceptDispatcher.AcceptAsync(applicationId);
        return Ok(outcome);
    }

    [Authorize]
    [HttpPost("complete")]
    public async Task<IActionResult> Complete(
        [FromQuery] int concertId,
        [FromServices] IFinishedDispatcher finishedDispatcher)
    {
        var result = await finishedDispatcher.FinishedAsync(concertId);

        if (result.IsFailed)
            return BadRequest(result.Errors.Select(e => e.Message));

        return Ok(result.Value);
    }
}
