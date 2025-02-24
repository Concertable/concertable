using Application.DTOs;
using Application.Interfaces;
using Core.Parameters;
using Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService reviewService;

        public ReviewController(IReviewService reviewService)
        {
            this.reviewService = reviewService;
        }

        [HttpGet("venue/{id}")]
        public async Task<ActionResult<PaginationResponse<ReviewDto>>> GetByVenueId(int id, PaginationParams pageParams)
        {
            return Ok();
        }

        [HttpGet("artist/{id}")]
        public async Task<ActionResult<PaginationResponse<ReviewDto>>> GetByArtistId(int id, PaginationParams pageParams)
        {
            return Ok();
        }

        [HttpGet("event/{id}")]
        public async Task<ActionResult<PaginationResponse<ReviewDto>>> GetByEventId(int id, PaginationParams pageParams)
        {
            return Ok();
        }

        [HttpGet("venue/{id}/summary")]
        public async Task<ActionResult<ReviewSummaryDto>> GetSummaryByVenueId(int id)
        {
            return Ok(await reviewService.GetSummaryByVenueIdAsync(id));
        }

        [HttpGet("artist/{id}/summary")]
        public async Task<ActionResult<ReviewSummaryDto>> GetSummaryByArtistId(int id)
        {
            return Ok(await reviewService.GetSummaryByArtistIdAsync(id));
        }

        [HttpGet("event/{id}/summary")]
        public async Task<ActionResult<ReviewSummaryDto>> GetSummaryByEventId(int id)
        {
            return Ok(await reviewService.GetSummaryByEventIdAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReviewDto review)
        {
            return Created();
        }
    }
}
