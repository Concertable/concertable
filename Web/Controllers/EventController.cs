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
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly IEventService eventService;
        private readonly IHubContext<EventHub> hubContext;

        public EventController(IEventService eventService, IHubContext<EventHub> hubContext) 
        {
            this.eventService = eventService;
            this.hubContext = hubContext;
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

        [HttpGet("headers/local/user")]
        public async Task<ActionResult<IEnumerable<EventHeaderDto>>> GetLocalHeadersForUser([FromQuery]bool orderByRecent, [FromQuery]int? take)
        {
            return Ok(await eventService.GetLocalHeadersForUserAsync(orderByRecent, take));
        }

        [HttpPut("post/{id}")]
        public async Task<ActionResult<EventDto>> Post([FromBody]EventDto eventDto)
        {
            var eventResponse = await eventService.PostAsync(eventDto);

            foreach (var userId in eventResponse.UserIds)
            {
                await hubContext.Clients.User(userId.ToString())
                    .SendAsync("EventPosted", eventResponse.Event);
            }

            return Ok(eventResponse.Event);
        }

        [HttpPut("test-signalr")]
        public async Task<IActionResult> TestSignalR([FromBody] int userId)
        {
            await hubContext.Clients.User(userId.ToString()).SendAsync("EventPosted",
                new EventDto { });

            return Ok($"SignalR test message sent to User {userId}");
        }

    }
}
