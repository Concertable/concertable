using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Auth;
using Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;
using Infrastructure.Data;
using Infrastructure.Data.Identity;
using Microsoft.AspNetCore.Authorization;
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

    [HttpPost("pay")]
    public async Task<IActionResult> Pay(
        [FromQuery] int applicationId,
        [FromServices] ApplicationDbContext context,
        [FromServices] IConcertService concertService)
    {
        var info = await context.ConcertApplications
            .Where(a => a.Id == applicationId)
            .Select(a => new
            {
                VenueUserId = a.Opportunity.Venue.UserId,
                VenueUserEmail = a.Opportunity.Venue.User.Email,
                ArtistUserId = a.Artist.UserId
            })
            .FirstOrDefaultAsync();

        if (info is null)
            return NotFound();

        await concertService.CompleteAsync(new PurchaseCompleteDto
        {
            EntityId = applicationId,
            TransactionId = $"dev_{Guid.NewGuid()}",
            FromUserId = info.VenueUserId,
            FromEmail = info.VenueUserEmail ?? string.Empty,
            ToUserId = info.ArtistUserId
        });

        return Ok();
    }

    [Authorize]
    [HttpPost("accept")]
    public async Task<IActionResult> Accept(
        [FromQuery] int applicationId,
        [FromServices] IAcceptProcessor acceptProcessor)
    {
        await acceptProcessor.AcceptAsync(applicationId);
        return Ok();
    }

    [Authorize]
    [HttpPost("settle")]
    public async Task<IActionResult> Settle(
        [FromQuery] int concertId,
        [FromServices] ISettlementProcessor settlementProcessor)
    {
        await settlementProcessor.SettleAsync(concertId);
        return Ok();
    }

    [Authorize]
    [HttpPost("complete")]
    public async Task<IActionResult> Complete(
        [FromQuery] int concertId,
        [FromServices] ICompleteProcessor completeProcessor)
    {
        await completeProcessor.CompleteAsync(concertId);
        return Ok();
    }
}
