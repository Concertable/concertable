using Concertable.Concert.Api.Mappers;
using Concertable.Concert.Api.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Concert.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
internal class OpportunityController : ControllerBase
{
    private readonly IOpportunityService opportunityService;
    private readonly IOpportunityResponseMapper mapper;

    public OpportunityController(IOpportunityService opportunityService, IOpportunityResponseMapper mapper)
    {
        this.opportunityService = opportunityService;
        this.mapper = mapper;
    }

    [HttpGet("active/venue/{id}")]
    public async Task<IActionResult> GetActiveByVenueId(int id, [FromQuery] PageParams pageParams)
    {
        var page = await opportunityService.GetActiveByVenueIdAsync(id, pageParams);
        return Ok(mapper.ToResponses(page));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OpportunityResponse>> GetById(int id)
    {
        var opportunity = await opportunityService.GetByIdAsync(id);
        return Ok(mapper.ToResponse(opportunity));
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OpportunityRequest request)
    {
        var opportunity = await opportunityService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = opportunity.Id }, mapper.ToResponse(opportunity));
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
        return Ok(mapper.ToResponse(opportunity));
    }

    [HttpGet("{id}/ownership")]
    public async Task<IActionResult> IsOwner(int id)
    {
        return Ok(await opportunityService.OwnsOpportunityAsync(id));
    }

    [HttpGet("by-application/{applicationId}/ownership")]
    public async Task<IActionResult> IsOwnerByApplicationId(int applicationId)
    {
        return Ok(await opportunityService.OwnsOpportunityByApplicationIdAsync(applicationId));
    }
}
