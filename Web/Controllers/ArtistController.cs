using Core.Entities;
using Application.Interfaces;
using Core.Parameters;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Core.ModelBinders;

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
        public async Task<ActionResult<IEnumerable<ArtistHeaderDto>>> GetHeaders([ModelBinder(BinderType = typeof(SearchParamsModelBinder))][FromQuery] SearchParams searchParams)
        {
            return Ok(await artistService.GetHeadersAsync(searchParams));
        }

        [Authorize(Roles = "ArtistManager, Admin")]
        [HttpGet("user")]
        public async Task<ActionResult<ArtistDto?>> GetDetailsForCurrentUser()
        {
            return Ok(await artistService.GetDetailsForCurrentUserAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]CreateArtistDto createArtistDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //TODO: Talk about security benefits of passing user seperate instead of through the client side
            var artistDto = await artistService.CreateAsync(createArtistDto);
            return CreatedAtAction(nameof(GetDetailsById), new { Id = artistDto.Id }, artistDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ArtistDto artistDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(await artistService.UpdateAsync(artistDto));
        }
    }
}
