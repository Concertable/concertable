using Core.Entities;
using Application.Interfaces;
using Application.Interfaces.Concert;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Responses;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConcertApplicationController : ControllerBase
{
    private readonly IConcertApplicationService applicationService;
    private readonly IConcertApplicationValidator applicationValidator;
    private readonly IArtistService artistService;
    private readonly IOwnershipService ownershipService;
    private readonly ICurrentUser currentUser;

    public ConcertApplicationController(
        IConcertApplicationService applicationService,
        IConcertApplicationValidator applicationValidator,
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
    [HttpGet("all/{id}")]
    public async Task<ActionResult<IEnumerable<ConcertApplicationDto>>> GetAllByOpportunityId(int id)
    {
        return Ok(await applicationService.GetByOpportunityIdAsync(id));
    }

    [Authorize(Roles = "ArtistManager")]
    [HttpPost("{opportunityId}")]
    public async Task<IActionResult> Apply(int opportunityId)
    {
        await applicationService.ApplyAsync(opportunityId);
        return NoContent();
    }

    [HttpGet("artist/pending")]
    [Authorize(Roles = "ArtistManager")]
    public async Task<ActionResult<IEnumerable<ArtistConcertApplicationDto>>> GetPendingForArtist()
    {
        return Ok(await applicationService.GetPendingForArtistAsync());
    }

    [HttpGet("artist/recently-denied")]
    [Authorize(Roles = "ArtistManager")]
    public async Task<ActionResult<IEnumerable<ArtistConcertApplicationDto>>> GetRecentDeniedForArtist()
    {
        return Ok(await applicationService.GetRecentDeniedForArtistAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ConcertApplicationEntity>> GetById(int id)
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

        if (!result.IsValid)
            return BadRequest(result.Errors);

        return Ok(true);
    }

    [Authorize(Roles = "VenueManager")]
    [HttpGet("can-accept/{applicationId}")]
    public async Task<ActionResult<bool>> CanAcceptApplication(int applicationId)
    {
        var result = await applicationValidator.CanAcceptAsync(applicationId);

        if (!result.IsValid)
            return BadRequest(result.Errors);

        return Ok(true);
    }

    [HttpGet("is-owner/{id}")]
    public async Task<ActionResult<bool>> IsOwner(int id)
    {
        return Ok(await ownershipService.OwnsOpportunityByApplicationId(id));
    }

}
