using Core.Entities;
using Application.Interfaces;
using Core.Parameters;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Core.Responses;

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
        public async Task<ActionResult<PaginationResponse<VenueHeaderDto>>> GetHeaders([FromQuery] SearchParams? searchParams)
        {
            return Ok(await venueService.GetHeadersAsync(searchParams));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VenueDto>> GetDetailsById(int id)
        {
            return Ok(await venueService.GetDetailsByIdAsync(id));
        }

        [Authorize(Roles = "VenueManager, Admin")]
        [HttpGet("user")]
        public async Task<ActionResult<VenueDto?>> GetDetailsForCurrentUser()
        {
            return Ok(await venueService.GetDetailsForCurrentUserAsync());
            
        }

        [Authorize(Roles = "VenueManager, Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVenueDto createVenueDto)
        {
            //TODO: Talk about security benefits of passing user seperate instead of through the client side
            var venueDto = await venueService.CreateAsync(createVenueDto);
            return CreatedAtAction(nameof(GetDetailsById), new {Id = venueDto.Id}, venueDto);
        }
    }
}
