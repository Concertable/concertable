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

        [HttpGet("headers/amount/{amount}")]
        public async Task<ActionResult<PaginationResponse<VenueHeaderDto>>> GetHeaders(int amount)
        {
            return Ok(await eventService.GetHeadersAsync(amount));
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

        [HttpGet("history/venue/{id}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetHistoryByVenueId(int id)
        {
            return Ok(await eventService.GetHistoryByVenueIdAsync(id));
        }

        [HttpGet("history/artist/{id}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetHistoryByArtistId(int id)
        {
            return Ok(await eventService.GetHistoryByArtistIdAsync(id));
        }

        [HttpGet("unposted/venue/{id}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetUnpostedByVenueId(int id)
        {
            return Ok(await eventService.GetUnpostedByVenueIdAsync(id));
        }

        [HttpGet("unposted/artist/{id}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetUnpostedByArtistId(int id)
        {
            return Ok(await eventService.GetUnpostedByArtistIdAsync(id));
        }

        [Authorize(Roles = "VenueManager")]
        [HttpPost("book")]
        public async Task<ActionResult<ListingApplicationPurchaseResponse>> Book(EventBookingParams bookingParams)
        {
            return Ok(await eventService.BookAsync(bookingParams));
        }

        [HttpGet("headers/recommended")]
        public async Task<ActionResult<IEnumerable<EventHeaderDto>>> GetRecommendedHeaders()
        {
            return Ok(await eventService.GetRecommendedHeadersAsync());
        }

        [Authorize(Roles = "VenueManager")]
        [HttpPut("{id}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> Update([FromBody]EventDto eventDto)
        {
            return Ok(await eventService.UpdateAsync(eventDto));
        }

        [Authorize(Roles = "VenueManager")]
        [HttpPut("post/{id}")]
        public async Task<ActionResult<EventDto>> Post([FromBody]EventDto eventDto)
        {
            var eventResponse = await eventService.PostAsync(eventDto);

            foreach (var userId in eventResponse.UserIds)
            {
                await hubContext.Clients.User(userId.ToString())
                    .SendAsync("EventPosted", eventResponse.EventHeader);
            }

            return Ok(eventResponse.Event);
        }

        [HttpGet("test-signalr")]
        public async Task<IActionResult> TestSignalR(
            [FromQuery] int userId,
            [FromQuery] string? name,
            [FromQuery] string? imageUrl)
        {
            var eventHeaderDto = new EventHeaderDto
            {
                Id = 1,
                Name = name ?? "The Rockers performing at the Grand Venue",
                ImageUrl = imageUrl ?? "rockers.jpg",
                County = "Surrey",
                Town = "Ashtead",
                Latitude = 53.4808,
                Longitude = -2.2426,
                Rating = 4.7,
                StartDate = new DateTime(2025, 8, 10, 17, 0, 0),
                EndDate = new DateTime(2025, 8, 10, 23, 30, 0),
                DatePosted = DateTime.UtcNow
            };

            await hubContext.Clients.User(userId.ToString())
                .SendAsync("EventPosted", eventHeaderDto);

            return Ok($"SignalR test message sent to User {userId}");
        }


    }
}
