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
    [HttpPost("complete")]
    public async Task<IActionResult> Complete(
        [FromQuery] int concertId,
        [FromServices] ICompleteProcessor completeProcessor)
    {
        await completeProcessor.CompleteAsync(concertId);
        return Ok();
    }
}
