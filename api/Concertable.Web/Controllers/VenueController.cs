using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Requests;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VenueController : ControllerBase
{
    private readonly IVenueService venueService;
    private readonly IOwnershipService ownershipService;

    public VenueController(IVenueService venueService, IOwnershipService ownershipService)
    {
        this.venueService = venueService;
        this.ownershipService = ownershipService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VenueDto>> GetDetailsById(int id)
    {
        return Ok(await venueService.GetDetailsByIdAsync(id));
    }

    [Authorize(Roles = "VenueManager")]
    [HttpGet("user")]
    public async Task<ActionResult<VenueDto?>> GetDetailsForCurrentUser()
    {
        return Ok(await venueService.GetDetailsForCurrentUserAsync());
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateVenueRequest request)
    {
        var venueDto = await venueService.CreateAsync(request);
        return CreatedAtAction(nameof(GetDetailsById), new { Id = venueDto.Id }, venueDto);
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateVenueRequest request)
    {
        return Ok(await venueService.UpdateAsync(id, request));
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        await venueService.ApproveAsync(id);
        return NoContent();
    }

    [HttpGet("is-owner/{id}")]
    public async Task<ActionResult<bool>> IsOwner(int id)
    {
        return Ok(await ownershipService.OwnsVenueAsync(id));
    }
}
