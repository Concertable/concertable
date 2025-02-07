using Core.Entities;
using Application.Interfaces;
using Core.Parameters;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VenueController : ControllerBase
    {
        private readonly IVenueService venueService;

        public VenueController(IVenueService venueService)
        {
            this.venueService = venueService;
        }

        [HttpGet("headers")]
        public async Task<ActionResult<IEnumerable<Venue>>> GetHeaders([FromQuery] SearchParams searchParams)
        {
            return Ok(await venueService.GetHeadersAsync(searchParams));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Venue>> GetDetailsById(int id)
        {
            return Ok(await venueService.GetDetailsByIdAsync(id));
        }

        [Authorize(Roles = "VenueManager, Admin")]
        [HttpGet("user-venue")]
        public async Task<ActionResult<VenueDto?>> GetUserVenue()
        {
            var venue = await venueService.GetUserVenueAsync();
            return Ok(new VenueDto()
            {
                Id = venue.Id,
                Name = venue.Name,
                About = venue.About,
                Coordinates = new CoordinatesDto()
                {
                    Latitude = venue.Latitude,
                    Longitude = venue.Longitude,
                },
                ImageUrl = venue.ImageUrl,
                County = venue.County,
                Town = venue.Town,
                Approved = venue.Approved
            } ?? null);
        }

        [Authorize(Roles = "VenueManager, Admin")]
        [HttpPost]
        public async Task<ActionResult<Venue>> Create([FromBody] VenueDto venueDto)
        {
            //TODO: Talk about security benefits of passing user seperate instead of through the client side
            var venue = new Venue()
            {
                Name = venueDto.Name,
                About = venueDto.About,
                Longitude = venueDto.Coordinates.Longitude,
                Latitude = venueDto.Coordinates.Latitude,
                ImageUrl = venueDto.ImageUrl,
                County = venueDto.County,
                Town = venueDto.Town,
                Approved = venueDto.Approved
            };
            venueService.Create(venue);
            return CreatedAtAction(nameof(GetDetailsById), new {Id = venue.Id}, venueDto);
        }
    }
}
