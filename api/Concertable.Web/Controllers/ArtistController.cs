using Concertable.Application.Interfaces;
using Concertable.Web.Mappers;
using Concertable.Web.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult<ArtistDetailsResponse>> GetDetailsById(int id)
    {
        return Ok((await artistService.GetDetailsByIdAsync(id)).ToDetailsResponse());
    }

    [Authorize(Roles = "ArtistManager")]
    [HttpGet("user")]
    public async Task<ActionResult<ArtistDetailsResponse>> GetDetailsForCurrentUser()
    {
        return Ok((await artistService.GetDetailsForCurrentUserAsync()).ToDetailsResponse());
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
