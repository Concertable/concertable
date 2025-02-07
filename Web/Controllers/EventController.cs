using Core.Entities;
using Core.Interfaces;
using Core.Parameters;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using Web.DTOs;

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
        public async Task<ActionResult<IEnumerable<EventDto>>> GetHeaders([FromQuery] SearchParams searchParams)
        {
            var eventHeaders = await eventService.GetHeadersAsync(searchParams);
            var headersDto = eventHeaders.Select(header => new EventHeaderDto
            {
                Id = header.Id,
                ImageUrl = header.ImageUrl,
            });
            return Ok(headersDto);
        }

        [HttpGet("venue/{id}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetUpcomingByVenueId(int id)
        {
            var events = await eventService.GetUpcomingByVenueIdAsync(id);
            var eventDtos = events.Select(e => new EventDto
            {
                Id = e.Id,
                Name = e.Name,
                About = e.About,
                Price = e.Price,
                ImageUrl = e.ImageUrl,
                StartDate = e.Register.Listing.StartDate,
                EndDate = e.Register.Listing.EndDate,
                TotalTickets = e.TotalTickets,
                AvailableTickets = e.AvailableTickets,
            }).ToList();
            return Ok(eventDtos);
        }

    }
}
