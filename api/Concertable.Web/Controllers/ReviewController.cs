using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Concertable.Core.Enums;
using Concertable.Core.Parameters;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly IReviewServiceFactory reviewServiceFactory;
    private readonly IReviewValidator reviewValidator;

    public ReviewController(IReviewServiceFactory reviewServiceFactory, IReviewValidator reviewValidator)
    {
        this.reviewServiceFactory = reviewServiceFactory;
        this.reviewValidator = reviewValidator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateReviewRequest request)
    {
        var service = reviewServiceFactory.Create(ReviewType.Concert);
        var review = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetByConcertId), new { id = review.Id }, review);
    }

    [HttpGet("venue/summary/{id}")]
    public async Task<ActionResult<ReviewSummaryDto>> GetSummaryByVenueId(int id) =>
        Ok(await reviewServiceFactory.Create(ReviewType.Venue).GetSummaryAsync(id));

    [HttpGet("artist/summary/{id}")]
    public async Task<ActionResult<ReviewSummaryDto>> GetSummaryByArtistId(int id) =>
        Ok(await reviewServiceFactory.Create(ReviewType.Artist).GetSummaryAsync(id));

    [HttpGet("concert/summary/{id}")]
    public async Task<ActionResult<ReviewSummaryDto>> GetSummaryByConcertId(int id) =>
        Ok(await reviewServiceFactory.Create(ReviewType.Concert).GetSummaryAsync(id));

    [HttpGet("venue/{id}")]
    public async Task<ActionResult<IPagination<ReviewDto>>> GetByVenueId(int id, [FromQuery] PageParams pageParams) =>
        Ok(await reviewServiceFactory.Create(ReviewType.Venue).GetAsync(id, pageParams));

    [HttpGet("artist/{id}")]
    public async Task<ActionResult<IPagination<ReviewDto>>> GetByArtistId(int id, [FromQuery] PageParams pageParams) =>
        Ok(await reviewServiceFactory.Create(ReviewType.Artist).GetAsync(id, pageParams));

    [HttpGet("concert/{id}")]
    public async Task<ActionResult<IPagination<ReviewDto>>> GetByConcertId(int id, [FromQuery] PageParams pageParams) =>
        Ok(await reviewServiceFactory.Create(ReviewType.Concert).GetAsync(id, pageParams));

    [HttpGet("concert/can-review/{concertId}")]
    public async Task<ActionResult<bool>> CanUserReviewConcertId(int concertId) =>
        Ok(await reviewValidator.CanUserReviewConcertAsync(concertId));

    [HttpGet("artist/can-review/{artistId}")]
    public async Task<ActionResult<bool>> CanUserReviewArtistId([FromQuery] int artistId) =>
        Ok(await reviewValidator.CanUserReviewArtistAsync(artistId));

    [HttpGet("venue/can-review/{venueId}")]
    public async Task<ActionResult<bool>> CanUserReviewVenueId([FromQuery] int venueId) =>
        Ok(await reviewValidator.CanUserReviewVenueAsync(venueId));
}
