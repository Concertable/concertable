using Concertable.Core.Parameters;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Venue.Api.Controllers;

[ApiController]
[Route("api/venues/{venueId}/reviews")]
internal class VenueReviewsController : ControllerBase
{
    private readonly IVenueReviewService reviewService;

    public VenueReviewsController(IVenueReviewService reviewService)
    {
        this.reviewService = reviewService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ReviewSummaryDto>> GetSummary(int venueId) =>
        Ok(await reviewService.GetSummaryAsync(venueId));

    [HttpGet]
    public async Task<ActionResult<IPagination<ReviewDto>>> Get(int venueId, [FromQuery] PageParams pageParams) =>
        Ok(await reviewService.GetAsync(venueId, pageParams));

    [HttpGet("eligibility")]
    public async Task<ActionResult<bool>> CanCurrentUserReview(int venueId) =>
        Ok(await reviewService.CanCurrentUserReviewAsync(venueId));
}