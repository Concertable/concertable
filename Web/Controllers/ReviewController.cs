using Application.DTOs;
using Application.Interfaces;
using Core.Parameters;
using Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Infrastructure.Factories;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService reviewService;
        private readonly IReviewServiceMethodFactory reviewFactory;

        public ReviewController(IReviewService reviewService, IReviewServiceMethodFactory reviewFactory)
        {
            this.reviewService = reviewService;
            this.reviewFactory = reviewFactory;
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReviewDto review)
        {
            return Created();
        }

        [HttpGet("venue/summary/{id}")]
        public async Task<ActionResult<ReviewSummaryDto>> GetSummaryByVenueId(int id)
        {
            return Ok(await reviewService.GetSummaryByVenueIdAsync(id));
        }

        [HttpGet("artist/summary/{id}")]
        public async Task<ActionResult<ReviewSummaryDto>> GetSummaryByArtistId(int id)
        {
            return Ok(await reviewService.GetSummaryByArtistIdAsync(id));
        }

        [HttpGet("event/summary/{id}")]
        public async Task<ActionResult<ReviewSummaryDto>> GetSummaryByEventId(int id)
        {
            return Ok(await reviewService.GetSummaryByEventIdAsync(id));
        }

        [HttpGet("venue/{id}")]
        public async Task<ActionResult<PaginationResponse<ReviewDto>>> GetByVenueId(int id, [FromQuery] PaginationParams pageParams)
        {
            return Ok(await reviewService.GetByVenueIdAsync(id, pageParams));
        }

        [HttpGet("artist/{id}")]
        public async Task<ActionResult<PaginationResponse<ReviewDto>>> GetByArtistId(int id, [FromQuery] PaginationParams pageParams)
        {
            return Ok(await reviewService.GetByArtistIdAsync(id, pageParams));
        }

        [HttpGet("event/{id}")]
        public async Task<ActionResult<PaginationResponse<ReviewDto>>> GetByEventId(int id, [FromQuery]PaginationParams pageParams)
        {
            return Ok(await reviewService.GetByEventIdAsync(id, pageParams));
        }

        [HttpGet("event/can-review/{eventId}")]
        public async Task<ActionResult<bool>> CanUserReviewEventId( int eventId)
        {
            return Ok(await reviewService.CanUserReviewEventIdAsync(eventId));
        }

        [HttpGet("artist/can-review/{artistId}")]
        public async Task<ActionResult<bool>> CanUserReviewArtistId([FromQuery] int artistId)
        {
            return Ok(await reviewService.CanUserReviewArtistIdAsync(artistId));
        }

        [HttpGet("venue/can-review/{venueId}")]
        public async Task<ActionResult<bool>> CanUserReviewVenueId([FromQuery] int venueId)
        {
            return Ok(await reviewService.CanUserReviewVenueIdAsync(venueId));
        }

    }
}
