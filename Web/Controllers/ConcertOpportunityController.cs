using Core.Entities;
using Application.Interfaces;
using Core.Parameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;

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
    public async Task<IActionResult> Create([FromBody] ConcertOpportunityDto opportunityDto)
    {
        await opportunityService.CreateAsync(opportunityDto);
        return Created();
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPost("bulk")]
    public async Task<IActionResult> CreateMultiple([FromBody] IEnumerable<ConcertOpportunityDto> opportunitiesDto)
    {
        await opportunityService.CreateMultipleAsync(opportunitiesDto);
        return Created();
    }

    [HttpGet("is-owner/{id}")]
    public async Task<IActionResult> IsOwner(int id)
    {
        return Ok(await ownershipService.OwnsOpportunityAsync(id));
    }
}
