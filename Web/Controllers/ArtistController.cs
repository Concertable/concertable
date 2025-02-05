using Core.Entities;
using Core.Interfaces;
using Core.Parameters;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

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
            var venues = await artistService.GetHeadersAsync(searchParams);
            return Ok(venues);
        }
    }
}
