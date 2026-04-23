using Concertable.Concert.Application.Interfaces.Reviews;
using Concertable.Concert.Application.Requests;
using Concertable.Core.Parameters;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Concert.Api.Controllers;

[ApiController]
[Route("api/Concert")]
internal class ConcertReviewController(IConcertReviewService reviewService) : ControllerBase
{
    [HttpPost("{id}/reviews")]
    public async Task<IActionResult> Create(int id, [FromBody] CreateReviewRequest request)
    {
        request.ConcertId = id;
        var review = await reviewService.CreateAsync(request);
        return CreatedAtAction(nameof(GetByConcertId), new { id }, review);
    }

    [HttpGet("{id}/reviews")]
    public async Task<ActionResult<IPagination<ReviewDto>>> GetByConcertId(int id, [FromQuery] PageParams pageParams) =>
        Ok(await reviewService.GetAsync(id, pageParams));

    [HttpGet("{id}/review-summary")]
    public async Task<ActionResult<ReviewSummaryDto>> GetSummaryByConcertId(int id) =>
        Ok(await reviewService.GetSummaryAsync(id));

    [HttpGet("{id}/can-review")]
    public async Task<ActionResult<bool>> CanCurrentUserReview(int id) =>
        Ok(await reviewService.CanCurrentUserReviewAsync(id));
}
