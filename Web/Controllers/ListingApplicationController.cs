using Core.Entities;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Responses;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListingApplicationController : ControllerBase
{
    private readonly IListingApplicationService listingApplicationService;
    private readonly IListingApplicationValidator applicationValidator;
    private readonly IArtistService artistService;
    private readonly IOwnershipService ownershipService;
    private readonly ICurrentUser currentUser;

    public ListingApplicationController(
        IListingApplicationService listingApplicationService,
        IListingApplicationValidator applicationValidator,
        IArtistService artistService,
        IOwnershipService ownershipService,
        ICurrentUser currentUser)
    {
        this.listingApplicationService = listingApplicationService;
        this.applicationValidator = applicationValidator;
        this.artistService = artistService;
        this.ownershipService = ownershipService;
        this.currentUser = currentUser;
    }

    [Authorize(Roles = "VenueManager")]
    [HttpGet("all/{id}")]
    public async Task<ActionResult<IEnumerable<ListingApplicationDto>>> GetAllForListingId(int id)
    {
        return Ok(await listingApplicationService.GetForListingIdAsync(id));
    }

    [Authorize(Roles = "ArtistManager")]
    [HttpPost("{listingId}")]
    public async Task<IActionResult> ApplyForListing(int listingId)
    {
        await listingApplicationService.ApplyForListingAsync(listingId);
        return NoContent();
    }

    [HttpGet("artist/pending")]
    [Authorize(Roles = "ArtistManager")]
    public async Task<ActionResult<IEnumerable<ArtistListingApplicationDto>>> GetPendingForArtist()
    {
        return Ok(await listingApplicationService.GetPendingForArtistAsync());
    }

    [HttpGet("artist/recently-denied")]
    [Authorize(Roles = "ArtistManager")]
    public async Task<ActionResult<IEnumerable<ArtistListingApplicationDto>>> GetRecentDeniedForArtist()
    {
        return Ok(await listingApplicationService.GetRecentDeniedForArtistAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ListingApplication>> GetById(int id)
    {
        return Ok(await listingApplicationService.GetByIdAsync(id));
    }

    [Authorize(Roles = "ArtistManager")]
    [HttpGet("can-apply/{listingId}")]
    public async Task<ActionResult<bool>> CanApplyForListing(int listingId)
    {
        var artist = await artistService.GetDetailsForCurrentUserAsync();

        if (artist is null)
            return NotFound("Artist not found");

        var result = await applicationValidator.CanApplyForListingAsync(listingId, artist.Id);

        if (!result.IsValid)
            return BadRequest(result.Errors);

        return Ok(true);
    }

    [Authorize(Roles = "VenueManager")]
    [HttpGet("can-accept/{applicationId}")]
    public async Task<ActionResult<bool>> CanAcceptApplication(int applicationId)
    {
        var result = await applicationValidator.CanAcceptListingApplicationAsync(applicationId);

        if (!result.IsValid)
            return BadRequest(result.Errors);

        return Ok(true);
    }

    [HttpGet("is-owner/{id}")]
    public async Task<ActionResult<bool>> IsOwner(int id)
    {
        return Ok(await ownershipService.OwnsListingByApplicationId(id));
    }

}
