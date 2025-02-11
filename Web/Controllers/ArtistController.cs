using Core.Entities;
using Application.Interfaces;
using Core.Parameters;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Web.Controllers
{

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
        public async Task<ActionResult<VenueDto>> GetDetailsById(int id)
        {
            return Ok(await artistService.GetDetailsByIdAsync(id));
        }

        [HttpGet("headers")]
        public async Task<ActionResult<IEnumerable<ArtistHeaderDto>>> GetHeaders([FromQuery] SearchParams searchParams)
        {
            return Ok(await artistService.GetHeadersAsync(searchParams));
        }

        [Authorize(Roles = "ArtistManager, Admin")]
        [HttpGet("user")]
        public async Task<ActionResult<ArtistDto?>> GetDetailsForCurrentUser()
        {
            return Ok(await artistService.GetDetailsForCurrentUserAsync());
        }
    }
}
