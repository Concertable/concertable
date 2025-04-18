using Core.Entities;
using Application.Interfaces;
using Core.Parameters;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Core.ModelBinders;
using Application.Responses;
using Application.Requests;

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

        [HttpGet("headers/amount/{amount}")]
        public async Task<ActionResult<PaginationResponse<VenueHeaderDto>>> GetHeaders(int amount)
        {
            return Ok(await artistService.GetHeadersAsync(amount));
        }

        [Authorize(Roles = "ArtistManager")]
        [HttpGet("user")]
        public async Task<ActionResult<ArtistDto?>> GetDetailsForCurrentUser()
        {
            return Ok(await artistService.GetDetailsForCurrentUserAsync());
        }

        [Authorize(Roles = "ArtistManager")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]ArtistCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //TODO: Talk about security benefits of passing user seperate instead of through the client side
            var artistDto = await artistService.CreateAsync(request.Artist, request.Image);
            return CreatedAtAction(nameof(GetDetailsById), new { Id = artistDto.Id }, artistDto);
        }

        [Authorize (Roles = "ArtistManager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ArtistUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(await artistService.UpdateAsync(request.Artist, request.Image));
        }
    }
}
