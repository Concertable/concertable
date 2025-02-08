using Core.Entities;
using Application.Interfaces;
using Core.Parameters;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using Application.DTOs;

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
        public async Task<ActionResult<IEnumerable<EventHeaderDto>>> GetHeaders([FromQuery] SearchParams searchParams)
        {
            return Ok(await eventService.GetHeadersAsync(searchParams));
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

    }
}
