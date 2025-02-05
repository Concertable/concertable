using Core.Entities;
using Core.Interfaces;
using Core.Parameters;
using Microsoft.AspNetCore.Mvc;
using Web.DTOs;

namespace Web.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class HeaderController : ControllerBase
    {
        private readonly IHeaderService headerService;

        public HeaderController(IHeaderService headerService)
        {
            this.headerService = headerService;
        }

        [HttpGet("venue")]
        public async Task<ActionResult<IEnumerable<VenueHeaderDto>>> GetVenueHeaders([FromQuery] SearchParams searchParams)
        {
            var venues = await headerService.GetVenueHeadersAsync(searchParams);
            var headersDto = venues.Select(header => new VenueHeaderDto
            {
                Id = header.Id,
                ImageUrl = header.ImageUrl,
            });
            return Ok(headersDto);
        }

        [HttpGet("artist")]
        public async Task<ActionResult<IEnumerable<ArtistHeaderDto>>> GetArtistHeaders([FromQuery] SearchParams searchParams)
        {
            return Ok(await headerService.GetArtistHeadersAsync(searchParams));
        }

        [HttpGet("event")]
        public async Task<ActionResult<IEnumerable<EventHeaderDto>>> GetEventHeaders([FromQuery] SearchParams searchParams)
        {
            return Ok(await headerService.GetEventHeadersAsync(searchParams));
        }
    }
}
