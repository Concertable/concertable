using Core.Entities;
using Application.Interfaces;
using Core.Parameters;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;

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
        public async Task<ActionResult<IEnumerable<ArtistHeaderDto>>> GetHeaders([FromQuery] SearchParams searchParams)
        {
            return Ok(await artistService.GetHeadersAsync(searchParams));
        }
    }
}
