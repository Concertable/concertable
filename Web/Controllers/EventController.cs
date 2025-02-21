using Core.Entities;
using Application.Interfaces;
using Core.Parameters;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using Application.DTOs;
using Core.ModelBinders;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly IEventService eventService;

        public EventController(IEventService eventService) 
        {
            this.eventService = eventService;
        }

        [HttpGet("headers")]
        public async Task<ActionResult<IEnumerable<EventHeaderDto>>> GetHeaders([ModelBinder(BinderType = typeof(SearchParamsModelBinder))][FromQuery] SearchParams searchParams)
        {
            return Ok(await eventService.GetHeadersAsync(searchParams));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VenueDto>> GetDetailsById(int id)
        {
            return Ok(await eventService.GetDetailsByIdAsync(id));
        }

        [HttpGet("upcoming/venue/{id}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetUpcomingByVenueId(int id)
        {
            return Ok(await eventService.GetUpcomingByVenueIdAsync(id));
        }

        [HttpGet("upcoming/artist/{id}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetUpcomingByArtistId(int id)
        {
            return Ok(await eventService.GetUpcomingByArtistIdAsync(id));
        }

        [HttpPost("application/{id}")]
        public async Task<IActionResult> CreateFromApplicationId(int id)
        {
            var eventDto = await eventService.CreateFromApplicationIdAsync(id);
            return CreatedAtAction(nameof(GetDetailsById), new { id = eventDto.Id }, eventDto);
        }

    }
}
