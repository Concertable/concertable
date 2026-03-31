using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Parameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Concertable.Application.DTOs;
using Concertable.Application.Requests;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConcertOpportunityController : ControllerBase
{
    private readonly IConcertOpportunityService opportunityService;
    private readonly IOwnershipService ownershipService;
    public ConcertOpportunityController(IConcertOpportunityService opportunityService, IOwnershipService ownershipService)
    {
        this.opportunityService = opportunityService;
        this.ownershipService = ownershipService;
    }

    [HttpGet("active/venue/{id}")]
    public async Task<IActionResult> GetActiveByVenueId(int id, [FromQuery] PageParams pageParams)
    {
        return Ok(await opportunityService.GetActiveByVenueIdAsync(id, pageParams));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ConcertOpportunityDto>> GetById(int id)
    {
        return Ok(await opportunityService.GetByIdAsync(id));
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ConcertOpportunityRequest request)
    {
        var opportunity = await opportunityService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = opportunity.Id }, opportunity);
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPost("bulk")]
    public async Task<IActionResult> CreateMultiple([FromBody] IEnumerable<ConcertOpportunityRequest> requests)
    {
        await opportunityService.CreateMultipleAsync(requests);
        return Created();
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ConcertOpportunityDto>> Update(int id, [FromBody] ConcertOpportunityRequest request)
    {
        return Ok(await opportunityService.UpdateAsync(id, request));
    }

    [HttpGet("is-owner/{id}")]
    public async Task<IActionResult> IsOwner(int id)
    {
        return Ok(await ownershipService.OwnsOpportunityAsync(id));
    }
}
