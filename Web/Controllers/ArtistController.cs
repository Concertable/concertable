using Core.Entities;
using Core.Interfaces;
using Core.Parameters;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Web.DTOs;

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

        [HttpGet("headers")]
        public async Task<ActionResult<IEnumerable<Venue>>> GetHeaders([FromQuery] SearchParams searchParams)
        {
            var artistHeaders = await artistService.GetHeadersAsync(searchParams);
            var headersDto = artistHeaders.Select(header => new ArtistHeaderDto
            {
                Id = header.Id,
                ImageUrl = header.ImageUrl,
            });
            return Ok(headersDto);
        }
    }
}
