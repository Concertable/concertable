using Application.Interfaces;
using Application.Interfaces.Auth;
using Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;
using Infrastructure.Data;
using Infrastructure.Data.Identity;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevController : ControllerBase
{
    [HttpPost("reset-db")]
    public async Task<IActionResult> ResetDb(
        [FromServices] ApplicationDbContext context,
        [FromServices] IPasswordHasher passwordHasher,
        [FromServices] IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("UseFakeExternalServices"))
            return NotFound();

        var dbName = context.Database.GetDbConnection().Database;
        await context.Database.ExecuteSqlRawAsync($"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
        await context.Database.EnsureDeletedAsync();
        await ApplicationDbInitializer.InitializeAsync(context, passwordHasher);
        return Ok("Database reset and reseeded.");
    }

    [HttpGet("debug")]
    public async Task<IActionResult> Debug(
        [FromQuery] int applicationId,
        [FromServices] ApplicationDbContext context)
    {
        var app = await context.ConcertApplications
            .Where(a => a.Id == applicationId)
            .Select(a => new { a.Id, a.OpportunityId, a.Status })
            .FirstOrDefaultAsync();

        ContractEntity? contract = app != null
            ? await context.Contracts.Where(c => c.Id == app.OpportunityId).FirstOrDefaultAsync()
            : null;

        return Ok(new { app, contractExists = contract != null, contractType = contract?.ContractType.ToString() });
    }

    [HttpPost("accept")]
    public async Task<IActionResult> Accept(
        [FromQuery] int applicationId,
        [FromServices] ApplicationDbContext context,
        [FromServices] IUserService userService,
        [FromServices] IAcceptProcessor acceptProcessor)
    {
        var venueManagerUserId = await context.ConcertApplications
            .Where(a => a.Id == applicationId)
            .Select(a => (int?)a.Opportunity.Venue.UserId)
            .FirstOrDefaultAsync();

        if (venueManagerUserId is null)
            return NotFound("Application or opportunity not found");

        await AuthenticateAsAsync(venueManagerUserId.Value, userService);
        await acceptProcessor.AcceptAsync(applicationId);
        return Ok();
    }

    [HttpPost("settle")]
    public async Task<IActionResult> Settle(
        [FromQuery] int concertId,
        [FromServices] ApplicationDbContext context,
        [FromServices] IUserService userService,
        [FromServices] ISettlementProcessor settlementProcessor)
    {
        var venueManagerUserId = await context.Concerts
            .Where(c => c.Id == concertId)
            .Select(c => (int?)c.Application.Opportunity.Venue.UserId)
            .FirstOrDefaultAsync();

        if (venueManagerUserId is null)
            return NotFound("Concert not found");

        await AuthenticateAsAsync(venueManagerUserId.Value, userService);
        await settlementProcessor.SettleAsync(concertId);
        return Ok();
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete(
        [FromQuery] int concertId,
        [FromServices] ApplicationDbContext context,
        [FromServices] IUserService userService,
        [FromServices] ICompleteProcessor completeProcessor)
    {
        var venueManagerUserId = await context.Concerts
            .Where(c => c.Id == concertId)
            .Select(c => (int?)c.Application.Opportunity.Venue.UserId)
            .FirstOrDefaultAsync();

        if (venueManagerUserId is null)
            return NotFound("Concert not found");

        await AuthenticateAsAsync(venueManagerUserId.Value, userService);
        await completeProcessor.CompleteAsync(concertId);
        return Ok();
    }

    private async Task AuthenticateAsAsync(int userId, IUserService userService)
    {
        var dto = await userService.GetUserByIdAsync(userId);
        var entity = await userService.GetUserEntityByIdAsync(userId);
        HttpContext.Items[nameof(CurrentUser)] = new CurrentUser(dto!, entity);
    }
}
