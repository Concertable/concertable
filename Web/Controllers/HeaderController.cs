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
            var venueHeaders = await headerService.GetVenueHeadersAsync(searchParams);
            var headersDto = venueHeaders.Select(header => new VenueHeaderDto
            {
                Id = header.Id,
                ImageUrl = header.ImageUrl,
            });
            return Ok(headersDto);
        }

        [HttpGet("artist")]
        public async Task<ActionResult<IEnumerable<ArtistHeaderDto>>> GetArtistHeaders([FromQuery] SearchParams searchParams)
        {
            var artistHeaders = await headerService.GetArtistHeadersAsync(searchParams);
            var headersDto = artistHeaders.Select(header => new ArtistHeaderDto
            {
                Id = header.Id,
                ImageUrl = header.ImageUrl,
            });
            return Ok(headersDto);
        }

        [HttpGet("event")]
        public async Task<ActionResult<IEnumerable<EventHeaderDto>>> GetEventHeaders([FromQuery] SearchParams searchParams)
        {
            var eventHeaders = await headerService.GetArtistHeadersAsync(searchParams);
            var headersDto = eventHeaders.Select(header => new EventHeaderDto
            {
                Id = header.Id,
                ImageUrl = header.ImageUrl,
            });
            return Ok(headersDto);
        }
    }
}
