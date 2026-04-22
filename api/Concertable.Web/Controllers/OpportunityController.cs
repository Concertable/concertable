using Concertable.Core.Parameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Concertable.Application.DTOs;
using Concertable.Application.Requests;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
internal class OpportunityController : ControllerBase
{
    private readonly IOpportunityService opportunityService;

    public OpportunityController(IOpportunityService opportunityService)
    {
        this.opportunityService = opportunityService;
    }

    [HttpGet("active/venue/{id}")]
    public async Task<IActionResult> GetActiveByVenueId(int id, [FromQuery] PageParams pageParams)
    {
        return Ok(await opportunityService.GetActiveByVenueIdAsync(id, pageParams));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OpportunityDto>> GetById(int id)
    {
        return Ok(await opportunityService.GetByIdAsync(id));
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OpportunityRequest request)
    {
        var opportunity = await opportunityService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = opportunity.Id }, opportunity);
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
    public async Task<ActionResult<OpportunityDto>> Update(int id, [FromBody] OpportunityRequest request)
    {
        return Ok(await opportunityService.UpdateAsync(id, request));
    }

    [HttpGet("is-owner/{id}")]
    public async Task<IActionResult> IsOwner(int id)
    {
        return Ok(await opportunityService.OwnsOpportunityAsync(id));
    }
}
