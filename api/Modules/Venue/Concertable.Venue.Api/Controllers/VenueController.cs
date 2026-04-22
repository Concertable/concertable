using Concertable.Venue.Api.Mappers;
using Concertable.Venue.Api.Responses;
using Concertable.Venue.Application.Interfaces;
using Concertable.Venue.Application.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Venue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
internal class VenueController : ControllerBase
{
    private readonly IVenueService venueService;

    public VenueController(IVenueService venueService)
    {
        this.venueService = venueService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VenueDetailsResponse>> GetDetailsById(int id)
    {
        return Ok((await venueService.GetDetailsByIdAsync(id)).ToDetailsResponse());
    }

    [Authorize(Roles = "VenueManager")]
    [HttpGet("user")]
    public async Task<ActionResult<VenueDetailsResponse>> GetDetailsForCurrentUser()
    {
        return Ok((await venueService.GetDetailsForCurrentUserAsync()).ToDetailsResponse());
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
        return Ok(await venueService.OwnsVenueAsync(id));
    }
}
