using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Core.Entities;
using Core.Parameters;
using Application.Responses;
using Infrastructure.Helpers;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository reviewRepository;
        private readonly IMapper mapper;

        public ReviewService(IReviewRepository reviewRepository, IMapper mapper)
        {
            this.reviewRepository = reviewRepository;
            this.mapper = mapper;
        }

        public async Task AddAverageRatingsAsync(IEnumerable<ArtistHeaderDto> headers)
        {
            foreach (var h in headers)
            {
                h.Rating = await reviewRepository.GetAverageRatingByArtistIdAsync(h.Id);
            }
        }

        public async Task AddAverageRatingsAsync(IEnumerable<EventHeaderDto> headers)
        {
            foreach (var h in headers)
            {
                h.Rating = await reviewRepository.GetAverageRatingByEventIdAsync(h.Id);
            }
        }

        public async Task AddAverageRatingsAsync(IEnumerable<VenueHeaderDto> headers)
        {
            foreach (var h in headers)
            {
                h.Rating = await reviewRepository.GetAverageRatingByVenueIdAsync(h.Id);
            }
        }

        public Task<ReviewDto> CreateAsync(ReviewDto review)
        {
            throw new NotImplementedException();
        }

        private async Task<PaginationResponse<ReviewDto>> GetAsync(
            Func<PaginationParams, Task<PaginationResponse<Review>>> getAsync,
            PaginationParams pageParams)
        {
            var reviews = await getAsync(pageParams);
            var reviewsDto = mapper.Map<IEnumerable<ReviewDto>>(reviews.Data);

            return new PaginationResponse<ReviewDto>(
                reviewsDto,
                reviews.TotalCount,
                reviews.PageNumber,
                reviews.PageSize);
        }

        private async Task<ReviewSummaryDto> GetSummaryAsync(
            Func<PaginationParams, Task<PaginationResponse<Review>>> getAsync,
            Func<Task<double?>> getAverageRatingAsync)
        {
            var pageParams = PaginationHelper.CreateSummaryParams();
            var reviews = await GetAsync(getAsync, pageParams);
            var averageRating = await getAverageRatingAsync() ?? 0;

            return new ReviewSummaryDto
            {
                Reviews = reviews,
                AverageRating = averageRating
            };
        }

        public async Task<PaginationResponse<ReviewDto>> GetByArtistIdAsync(int id, PaginationParams pageParams)
        {
            return await GetAsync(p => reviewRepository.GetByArtistIdAsync(id, p), pageParams);
        }

        public async Task<PaginationResponse<ReviewDto>> GetByEventIdAsync(int id, PaginationParams pageParams)
        {
            return await GetAsync(p => reviewRepository.GetByEventIdAsync(id, p), pageParams);
        }

        public async Task<PaginationResponse<ReviewDto>> GetByVenueIdAsync(int id, PaginationParams pageParams)
        {
            return await GetAsync(p => reviewRepository.GetByVenueIdAsync(id, p), pageParams);
        }

        public async Task<ReviewSummaryDto> GetSummaryByArtistIdAsync(int id)
        {
            return await GetSummaryAsync(
                 p => reviewRepository.GetByArtistIdAsync(id, p),
                () => reviewRepository.GetAverageRatingByArtistIdAsync(id));
        }

        public async Task<ReviewSummaryDto> GetSummaryByEventIdAsync(int id)
        {
            return await GetSummaryAsync(
                 p => reviewRepository.GetByEventIdAsync(id, p),
                () => reviewRepository.GetAverageRatingByEventIdAsync(id));
        }

        public async Task<ReviewSummaryDto> GetSummaryByVenueIdAsync(int id)
        {
            return await GetSummaryAsync(
                 p => reviewRepository.GetByVenueIdAsync(id, p),
                () => reviewRepository.GetAverageRatingByVenueIdAsync(id));
        }
    }
}
