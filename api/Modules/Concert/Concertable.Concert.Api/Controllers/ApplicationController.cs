using Concertable.Artist.Contracts;
using Concertable.Authorization.Contracts;
using Concertable.Concert.Api.Mappers;
using Concertable.Concert.Api.Requests;
using Concertable.Concert.Api.Responses;
using Concertable.Shared;
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
    private readonly IApplicationResponseMapper mapper;

    public ApplicationController(
        IApplicationService applicationService,
        IApplicationValidator applicationValidator,
        IArtistModule artistModule,
        ICurrentUser currentUser,
        IApplicationResponseMapper mapper)
    {
        this.applicationService = applicationService;
        this.applicationValidator = applicationValidator;
        this.artistModule = artistModule;
        this.currentUser = currentUser;
        this.mapper = mapper;
    }

    [Authorize(Roles = "VenueManager")]
    [HttpGet("opportunity/{id}")]
    public async Task<ActionResult<IEnumerable<ApplicationResponse>>> GetAllByOpportunityId(int id)
    {
        var applications = await applicationService.GetByOpportunityIdAsync(id);
        return Ok(mapper.ToResponses(applications));
    }

    [Authorize(Roles = "ArtistManager")]
    [HttpPost("{opportunityId}")]
    public async Task<IActionResult> Apply(int opportunityId, [FromBody] ApplyRequest? request = null)
    {
        var application = request is not null
            ? await applicationService.ApplyAsync(opportunityId, request.PaymentMethodId)
            : await applicationService.ApplyAsync(opportunityId);
        return CreatedAtAction(nameof(GetById), new { id = application.Id }, mapper.ToResponse(application));
    }

    [HttpGet("artist/pending")]
    [Authorize(Roles = "ArtistManager")]
    public async Task<ActionResult<IEnumerable<ApplicationResponse>>> GetPendingForArtist()
    {
        var applications = await applicationService.GetPendingForArtistAsync();
        return Ok(mapper.ToResponses(applications));
    }

    [HttpGet("artist/recently-denied")]
    [Authorize(Roles = "ArtistManager")]
    public async Task<ActionResult<IEnumerable<ApplicationResponse>>> GetRecentDeniedForArtist()
    {
        var applications = await applicationService.GetRecentDeniedForArtistAsync();
        return Ok(mapper.ToResponses(applications));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApplicationResponse>> GetById(int id)
    {
        var application = await applicationService.GetByIdAsync(id);
        return Ok(mapper.ToResponse(application));
    }

    [Authorize(Roles = "ArtistManager")]
    [HttpGet("opportunity/{opportunityId}/eligibility")]
    public async Task<IActionResult> CanApply(int opportunityId)
    {
        var artistId = await artistModule.GetIdByUserIdAsync(currentUser.GetId());

        if (artistId is null)
            return BadRequest("You must create an Artist account before you apply for a concert opportunity");

        var result = await applicationValidator.CanApplyAsync(opportunityId, artistId.Value);

        if (result.IsFailed)
            return BadRequest(result.Errors.SelectMessages());

        return NoContent();
    }

    [Authorize(Roles = "VenueManager")]
    [HttpGet("can-accept/{applicationId}")]
    public async Task<IActionResult> CanAcceptApplication(int applicationId)
    {
        var result = await applicationValidator.CanAcceptAsync(applicationId);

        if (result.IsFailed)
            return BadRequest(result.Errors.SelectMessages());

        return NoContent();
    }

    [Authorize(Roles = "ArtistManager")]
    [HttpPost("opportunity/{opportunityId}/checkout")]
    public async Task<IActionResult> ApplyCheckout(int opportunityId)
    {
        var checkout = await applicationService.ApplyCheckoutAsync(opportunityId);
        return Ok(checkout);
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPost("{applicationId}/checkout")]
    public async Task<IActionResult> AcceptCheckout(int applicationId)
    {
        var checkout = await applicationService.AcceptCheckoutAsync(applicationId);
        return Ok(checkout);
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPost("{applicationId}/accept")]
    public async Task<IActionResult> Accept(int applicationId, [FromBody] AcceptRequest? request = null)
    {
        var outcome = request is not null
            ? await applicationService.AcceptAsync(applicationId, request.PaymentMethodId)
            : await applicationService.AcceptAsync(applicationId);
        return Ok(outcome);
    }

}
