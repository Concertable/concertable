using Concertable.Artist.Contracts;
using Concertable.Authorization.Contracts;
using Concertable.Concert.Api.Mappers;
using Concertable.Concert.Api.Requests;
using Concertable.Concert.Api.Responses;
using Concertable.User.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Concert.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
internal class ApplicationController : ControllerBase
{
    private readonly IApplicationService applicationService;
    private readonly IApplicationValidator applicationValidator;
    private readonly IArtistModule artistModule;
    private readonly ICurrentUser currentUser;
    private readonly IOpportunityService opportunityService;

    public ApplicationController(
        IApplicationService applicationService,
        IApplicationValidator applicationValidator,
        IArtistModule artistModule,
        ICurrentUser currentUser,
        IOpportunityService opportunityService)
    {
        this.applicationService = applicationService;
        this.applicationValidator = applicationValidator;
        this.artistModule = artistModule;
        this.currentUser = currentUser;
        this.opportunityService = opportunityService;
    }

    [Authorize(Roles = "VenueManager")]
    [HttpGet("opportunity/{id}")]
    public async Task<ActionResult<IEnumerable<ApplicationResponse>>> GetAllByOpportunityId(int id)
    {
        var applications = await applicationService.GetByOpportunityIdAsync(id);
        return Ok(applications.ToResponses());
    }

    [Authorize(Roles = "ArtistManager")]
    [HttpPost("{opportunityId}")]
    public async Task<IActionResult> Apply(int opportunityId)
    {
        var application = await applicationService.ApplyAsync(opportunityId);
        return CreatedAtAction(nameof(GetById), new { id = application.Id }, application.ToResponse());
    }

    [HttpGet("artist/pending")]
    [Authorize(Roles = "ArtistManager")]
    public async Task<ActionResult<IEnumerable<ApplicationResponse>>> GetPendingForArtist()
    {
        var applications = await applicationService.GetPendingForArtistAsync();
        return Ok(applications.ToResponses());
    }

    [HttpGet("artist/recently-denied")]
    [Authorize(Roles = "ArtistManager")]
    public async Task<ActionResult<IEnumerable<ApplicationResponse>>> GetRecentDeniedForArtist()
    {
        var applications = await applicationService.GetRecentDeniedForArtistAsync();
        return Ok(applications.ToResponses());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApplicationResponse>> GetById(int id)
    {
        var application = await applicationService.GetByIdAsync(id);
        return Ok(application.ToResponse());
    }

    [Authorize(Roles = "ArtistManager")]
    [HttpGet("can-apply/{opportunityId}")]
    public async Task<ActionResult<bool>> CanApply(int opportunityId)
    {
        var artistId = await artistModule.GetIdByUserIdAsync(currentUser.GetId());

        if (artistId is null)
            return NotFound("Artist not found");

        var result = await applicationValidator.CanApplyAsync(opportunityId, artistId.Value);

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
    [HttpPost("{applicationId}/checkout")]
    public async Task<IActionResult> Checkout(int applicationId)
    {
        return Ok(await applicationService.CheckoutAsync(applicationId));
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
        return Ok(await opportunityService.OwnsOpportunityByApplicationIdAsync(id));
    }
}
