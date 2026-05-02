using Concertable.Concert.Api.Mappers;
using Concertable.Concert.Api.Requests;
using Concertable.Concert.Api.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Concert.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
internal class OpportunityController : ControllerBase
{
    private readonly IOpportunityService opportunityService;
    private readonly IApplicationService applicationService;
    private readonly IOpportunityResponseMapper opportunityMapper;
    private readonly IApplicationResponseMapper applicationMapper;

    public OpportunityController(
        IOpportunityService opportunityService,
        IApplicationService applicationService,
        IOpportunityResponseMapper opportunityMapper,
        IApplicationResponseMapper applicationMapper)
    {
        this.opportunityService = opportunityService;
        this.applicationService = applicationService;
        this.opportunityMapper = opportunityMapper;
        this.applicationMapper = applicationMapper;
    }

    [HttpGet("active/venue/{id}")]
    public async Task<IActionResult> GetActiveByVenueId(int id, [FromQuery] PageParams pageParams)
    {
        return Ok(await opportunityService.GetActiveByVenueIdAsync(id, pageParams));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OpportunityResponse>> GetById(int id)
    {
        var opportunity = await opportunityService.GetByIdAsync(id);
        return Ok(opportunityMapper.ToResponse(opportunity));
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OpportunityRequest request)
    {
        var opportunity = await opportunityService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = opportunity.Id }, opportunityMapper.ToResponse(opportunity));
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPost("bulk")]
    public async Task<IActionResult> CreateMultiple([FromBody] IEnumerable<OpportunityRequest> requests)
    {
        await opportunityService.CreateMultipleAsync(requests);
        return Created();
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<OpportunityResponse>> Update(int id, [FromBody] OpportunityRequest request)
    {
        var opportunity = await opportunityService.UpdateAsync(id, request);
        return Ok(opportunityMapper.ToResponse(opportunity));
    }

    [HttpGet("is-owner/{id}")]
    public async Task<IActionResult> IsOwner(int id)
    {
        return Ok(await opportunityService.OwnsOpportunityAsync(id));
    }

    [Authorize(Roles = "ArtistManager")]
    [HttpPost("{opportunityId}/applications")]
    public async Task<IActionResult> Apply(int opportunityId, [FromBody] ApplyRequest? request = null)
    {
        var application = request is not null
            ? await applicationService.ApplyAsync(opportunityId, request.PaymentMethodId)
            : await applicationService.ApplyAsync(opportunityId);
        return CreatedAtAction(
            nameof(ApplicationController.GetById),
            "Application",
            new { id = application.Id },
            applicationMapper.ToResponse(application));
    }
}
