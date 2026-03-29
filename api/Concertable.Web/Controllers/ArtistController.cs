using Concertable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Concertable.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Concertable.Application.Requests;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtistController : ControllerBase
{
    private readonly IArtistService artistService;

    public ArtistController(IArtistService artistService)
    {
        this.artistService = artistService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ArtistDto>> GetDetailsById(int id)
    {
        return Ok(await artistService.GetDetailsByIdAsync(id));
    }

    [Authorize(Roles = "ArtistManager")]
    [HttpGet("user")]
    public async Task<ActionResult<ArtistDto?>> GetDetailsForCurrentUser()
    {
        return Ok(await artistService.GetDetailsForCurrentUserAsync());
    }

    [Authorize(Roles = "ArtistManager")]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateArtistRequest request)
    {
        var artistDto = await artistService.CreateAsync(request);
        return CreatedAtAction(nameof(GetDetailsById), new { Id = artistDto.Id }, artistDto);
    }

    [Authorize(Roles = "ArtistManager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateArtistRequest request)
    {
        return Ok(await artistService.UpdateAsync(id, request));
    }
}
