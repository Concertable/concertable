using Application.Interfaces;
using Application.Interfaces.Concert;
using Core.Parameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Requests;
using Application.Responses;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConcertController : ControllerBase
{
    private readonly IConcertService concertService;
    private readonly IHubContext<ConcertHub> hubContext;

    public ConcertController(IConcertService concertService, IHubContext<ConcertHub> hubContext)
    {
        this.concertService = concertService;
        this.hubContext = hubContext;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VenueDto>> GetDetailsById(int id)
    {
        return Ok(await concertService.GetDetailsByIdAsync(id));
    }

    [HttpGet("application/{applicationId}")]
    public async Task<ActionResult<VenueDto>> GetDetailsByApplicationId(int applicationId)
    {
        return Ok(await concertService.GetDetailsByApplicationIdAsync(applicationId));
    }

    [HttpGet("upcoming/venue/{id}")]
    public async Task<ActionResult<IEnumerable<ConcertDto>>> GetUpcomingByVenueId(int id)
    {
        return Ok(await concertService.GetUpcomingByVenueIdAsync(id));
    }

    [HttpGet("upcoming/artist/{id}")]
    public async Task<ActionResult<IEnumerable<ConcertDto>>> GetUpcomingByArtistId(int id)
    {
        return Ok(await concertService.GetUpcomingByArtistIdAsync(id));
    }

    [HttpGet("history/venue/{id}")]
    public async Task<ActionResult<IEnumerable<ConcertDto>>> GetHistoryByVenueId(int id)
    {
        return Ok(await concertService.GetHistoryByVenueIdAsync(id));
    }

    [HttpGet("history/artist/{id}")]
    public async Task<ActionResult<IEnumerable<ConcertDto>>> GetHistoryByArtistId(int id)
    {
        return Ok(await concertService.GetHistoryByArtistIdAsync(id));
    }

    [HttpGet("unposted/venue/{id}")]
    public async Task<ActionResult<IEnumerable<ConcertDto>>> GetUnpostedByVenueId(int id)
    {
        return Ok(await concertService.GetUnpostedByVenueIdAsync(id));
    }

    [HttpGet("unposted/artist/{id}")]
    public async Task<ActionResult<IEnumerable<ConcertDto>>> GetUnpostedByArtistId(int id)
    {
        return Ok(await concertService.GetUnpostedByArtistIdAsync(id));
    }

    [HttpGet("headers/recommended")]
    public async Task<ActionResult<IEnumerable<ConcertHeaderDto>>> GetRecommendedHeaders()
    {
        return Ok(await concertService.GetRecommendedHeadersAsync());
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ConcertDto>> Update(int id, [FromBody] UpdateConcertRequest request)
    {
        return Ok(await concertService.UpdateAsync(id, request));
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPut("post/{id}")]
    public async Task<ActionResult<ConcertDto>> Post(int id, [FromBody] UpdateConcertRequest request)
    {
        var concertResponse = await concertService.PostAsync(id, request);

        foreach (var userId in concertResponse.UserIds)
        {
            await hubContext.Clients.User(userId.ToString())
                .SendAsync("ConcertPosted", concertResponse.ConcertHeader);
        }

        return Ok(concertResponse.Concert);
    }

    [HttpGet("test-signalr")]
    public async Task<IActionResult> TestSignalR(
        [FromQuery] int userId,
        [FromQuery] string? name,
        [FromQuery] string? imageUrl)
    {
        var concertHeaderDto = new ConcertHeaderDto
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
            .SendAsync("ConcertPosted", concertHeaderDto);

        return Ok($"SignalR test message sent to User {userId}");
    }


}
