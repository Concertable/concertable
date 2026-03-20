using Core.Entities;
using Application.Interfaces;
using Application.Interfaces.Concert;
using Core.Parameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Requests;

namespace Web.Controllers;

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
    public async Task<ActionResult<IEnumerable<ConcertOpportunityDto>>> GetActiveByVenueId(int id)
    {
        return Ok(await opportunityService.GetActiveByVenueIdAsync(id));
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ConcertOpportunityRequest request)
    {
        await opportunityService.CreateAsync(request);
        return Created();
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPost("bulk")]
    public async Task<IActionResult> CreateMultiple([FromBody] IEnumerable<ConcertOpportunityRequest> requests)
    {
        await opportunityService.CreateMultipleAsync(requests);
        return Created();
    }

    [HttpGet("is-owner/{id}")]
    public async Task<IActionResult> IsOwner(int id)
    {
        return Ok(await ownershipService.OwnsOpportunityAsync(id));
    }
}
