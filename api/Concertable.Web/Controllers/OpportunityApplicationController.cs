using Concertable.Core.Entities;
using Concertable.Application.Interfaces;
using Concertable.Identity.Contracts;
using Concertable.Application.Interfaces.Concert;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Concertable.Application.DTOs;
using Concertable.Web.Requests;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OpportunityApplicationController : ControllerBase
{
    private readonly IOpportunityApplicationService applicationService;
    private readonly IOpportunityApplicationValidator applicationValidator;
    private readonly IArtistService artistService;
    private readonly IOwnershipService ownershipService;
    private readonly ICurrentUser currentUser;

    public OpportunityApplicationController(
        IOpportunityApplicationService applicationService,
        IOpportunityApplicationValidator applicationValidator,
        IArtistService artistService,
        IOwnershipService ownershipService,
        ICurrentUser currentUser)
    {
        this.applicationService = applicationService;
        this.applicationValidator = applicationValidator;
        this.artistService = artistService;
        this.ownershipService = ownershipService;
        this.currentUser = currentUser;
    }

    [Authorize(Roles = "VenueManager")]
    [HttpGet("opportunity/{id}")]
    public async Task<ActionResult<IEnumerable<OpportunityApplicationDto>>> GetAllByOpportunityId(int id)
    {
        return Ok(await applicationService.GetByOpportunityIdAsync(id));
    }

    [Authorize(Roles = "ArtistManager")]
    [HttpPost("{opportunityId}")]
    public async Task<IActionResult> Apply(int opportunityId)
    {
        var application = await applicationService.ApplyAsync(opportunityId);
        return CreatedAtAction(nameof(GetById), new { id = application.Id }, application);
    }

    [HttpGet("artist/pending")]
    [Authorize(Roles = "ArtistManager")]
    public async Task<ActionResult<IEnumerable<OpportunityApplicationDto>>> GetPendingForArtist()
    {
        return Ok(await applicationService.GetPendingForArtistAsync());
    }

    [HttpGet("artist/recently-denied")]
    [Authorize(Roles = "ArtistManager")]
    public async Task<ActionResult<IEnumerable<OpportunityApplicationDto>>> GetRecentDeniedForArtist()
    {
        return Ok(await applicationService.GetRecentDeniedForArtistAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OpportunityApplicationEntity>> GetById(int id)
    {
        return Ok(await applicationService.GetByIdAsync(id));
    }

    [Authorize(Roles = "ArtistManager")]
    [HttpGet("can-apply/{opportunityId}")]
    public async Task<ActionResult<bool>> CanApply(int opportunityId)
    {
        var artist = await artistService.GetDetailsForCurrentUserAsync();

        if (artist is null)
            return NotFound("Artist not found");

        var result = await applicationValidator.CanApplyAsync(opportunityId, artist.Id);

        if (result.IsFailed)
            return BadRequest(result.Errors.Select(e => e.Message));

        return Ok(true);
    }

    [Authorize(Roles = "VenueManager")]
    [HttpGet("can-accept/{applicationId}")]
    public async Task<ActionResult<bool>> CanAcceptApplication(int applicationId)
    {
        var result = await applicationValidator.CanAcceptAsync(applicationId);

        if (result.IsFailed)
            return BadRequest(result.Errors.Select(e => e.Message));

        return Ok(true);
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPost("accept/{applicationId}")]
    public async Task<IActionResult> Accept(int applicationId, [FromBody] AcceptApplicationRequest? request = null)
    {
        var outcome = await applicationService.AcceptAsync(applicationId, request?.PaymentMethodId);
        return Ok(outcome);
    }

    [HttpGet("is-owner/{id}")]
    public async Task<ActionResult<bool>> IsOwner(int id)
    {
        return Ok(await ownershipService.OwnsOpportunityByApplicationId(id));
    }

}
