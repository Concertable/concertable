using Core.Entities;
using Application.Interfaces;
using Core.Parameters;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using Application.DTOs;
using Core.ModelBinders;
using Application.Responses;

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

        [HttpGet("application/{applicationId}")]
        public async Task<ActionResult<VenueDto>> GetDetailsByApplicationId(int applicationId)
        {
            return Ok(await eventService.GetDetailsByApplicationIdAsync(applicationId));
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

        [HttpPost("book")]
        public async Task<ActionResult<ListingApplicationPurchaseResponse>> Book(EventBookingParams bookingParams)
        {
            return Ok(await eventService.BookAsync(bookingParams));
        }

    }
}
