using Concertable.Core.Parameters;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Artist.Api.Controllers;

[ApiController]
[Route("api/Artist")]
internal class ArtistReviewController(IArtistReviewService reviewService) : ControllerBase
{
    [HttpGet("{id}/review-summary")]
    public async Task<ActionResult<ReviewSummaryDto>> GetSummary(int id) =>
        Ok(await reviewService.GetSummaryAsync(id));

    [HttpGet("{id}/reviews")]
    public async Task<ActionResult<IPagination<ReviewDto>>> Get(int id, [FromQuery] PageParams pageParams) =>
        Ok(await reviewService.GetAsync(id, pageParams));

    [HttpGet("{id}/can-review")]
    public async Task<ActionResult<bool>> CanCurrentUserReview(int id) =>
        Ok(await reviewService.CanCurrentUserReviewAsync(id));
}
