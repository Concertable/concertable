using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Concertable.Core.Parameters;
using Concertable.Web.Handlers;
using Concertable.Web.Mappers;
using Concertable.Web.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConcertController : ControllerBase
{
    private readonly IConcertService concertService;
    private readonly IConcertNotificationService notificationService;
    private readonly IConcertPostedHandler concertPostedHandler;

    public ConcertController(
        IConcertService concertService,
        IConcertNotificationService notificationService,
        IConcertPostedHandler concertPostedHandler)
    {
        this.concertService = concertService;
        this.notificationService = notificationService;
        this.concertPostedHandler = concertPostedHandler;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ConcertDetailsResponse>> GetDetailsById(int id)
    {
        return Ok((await concertService.GetDetailsByIdAsync(id)).ToDetailsResponse());
    }

    [HttpGet("application/{applicationId}")]
    public async Task<ActionResult<ConcertDetailsResponse>> GetDetailsByApplicationId(int applicationId)
    {
        return Ok((await concertService.GetDetailsByApplicationIdAsync(applicationId)).ToDetailsResponse());
    }

    [HttpGet("upcoming/venue/{id}")]
    public async Task<ActionResult<IEnumerable<ConcertSummaryResponse>>> GetUpcomingByVenueId(int id)
    {
        return Ok((await concertService.GetUpcomingByVenueIdAsync(id)).ToSummaryResponses());
    }

    [HttpGet("upcoming/artist/{id}")]
    public async Task<ActionResult<IEnumerable<ConcertSummaryResponse>>> GetUpcomingByArtistId(int id)
    {
        return Ok((await concertService.GetUpcomingByArtistIdAsync(id)).ToSummaryResponses());
    }

    [HttpGet("history/venue/{id}")]
    public async Task<ActionResult<IEnumerable<ConcertSummaryResponse>>> GetHistoryByVenueId(int id)
    {
        return Ok((await concertService.GetHistoryByVenueIdAsync(id)).ToSummaryResponses());
    }

    [HttpGet("history/artist/{id}")]
    public async Task<ActionResult<IEnumerable<ConcertSummaryResponse>>> GetHistoryByArtistId(int id)
    {
        return Ok((await concertService.GetHistoryByArtistIdAsync(id)).ToSummaryResponses());
    }

    [HttpGet("unposted/venue/{id}")]
    public async Task<ActionResult<IEnumerable<ConcertSummaryResponse>>> GetUnpostedByVenueId(int id)
    {
        return Ok((await concertService.GetUnpostedByVenueIdAsync(id)).ToSummaryResponses());
    }

    [HttpGet("unposted/artist/{id}")]
    public async Task<ActionResult<IEnumerable<ConcertSummaryResponse>>> GetUnpostedByArtistId(int id)
    {
        return Ok((await concertService.GetUnpostedByArtistIdAsync(id)).ToSummaryResponses());
    }

    [HttpGet("headers/recommended")]
    public async Task<ActionResult<IEnumerable<ConcertHeaderDto>>> GetRecommendedHeaders()
    {
        return Ok(await concertService.GetRecommendedHeadersAsync());
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ConcertUpdateResponse>> Update(int id, [FromBody] UpdateConcertRequest request)
    {
        return Ok(await concertService.UpdateAsync(id, request));
    }

    [Authorize(Roles = "VenueManager")]
    [HttpPut("post/{id}")]
    public async Task<IActionResult> Post(int id, [FromBody] UpdateConcertRequest request)
    {
        var result = await concertService.PostAsync(id, request);
        await concertPostedHandler.HandleAsync(result);
        return NoContent();
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
            Rating = 4.7,
            StartDate = new DateTime(2025, 8, 10, 17, 0, 0),
            EndDate = new DateTime(2025, 8, 10, 23, 30, 0),
            DatePosted = DateTime.UtcNow
        };

        await notificationService.ConcertPostedAsync(userId.ToString(), concertHeaderDto);

        return Ok($"SignalR test message sent to User {userId}");
    }


}
