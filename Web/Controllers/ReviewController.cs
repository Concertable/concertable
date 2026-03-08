using Core.Interfaces;
﻿using Application.DTOs;
using Application.Interfaces;
using Core.Parameters;
using Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Application.Requests;

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

        [HttpPost]
        public async Task<IActionResult> Create(CreateReviewRequest request)
        {
            var createdReview = await reviewService.CreateAsync(request);
            return CreatedAtAction(nameof(GetByConcertId), new { id = createdReview.Id }, createdReview);
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

        [HttpGet("concert/summary/{id}")]
        public async Task<ActionResult<ReviewSummaryDto>> GetSummaryByConcertId(int id)
        {
            return Ok(await reviewService.GetSummaryByConcertIdAsync(id));
        }

        [HttpGet("venue/{id}")]
        public async Task<ActionResult<Pagination<ReviewDto>>> GetByVenueId(int id, [FromQuery] IPageParams pageParams)
        {
            return Ok(await reviewService.GetByVenueIdAsync(id, pageParams));
        }

        [HttpGet("artist/{id}")]
        public async Task<ActionResult<Pagination<ReviewDto>>> GetByArtistId(int id, [FromQuery] IPageParams pageParams)
        {
            return Ok(await reviewService.GetByArtistIdAsync(id, pageParams));
        }

        [HttpGet("concert/{id}")]
        public async Task<ActionResult<Pagination<ReviewDto>>> GetByConcertId(int id, [FromQuery]IPageParams pageParams)
        {
            return Ok(await reviewService.GetByConcertIdAsync(id, pageParams));
        }

        [HttpGet("concert/can-review/{concertId}")]
        public async Task<ActionResult<bool>> CanUserReviewConcertId( int concertId)
        {
            return Ok(await reviewService.CanUserReviewConcertIdAsync(concertId));
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
