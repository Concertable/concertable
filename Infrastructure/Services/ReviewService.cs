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
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository reviewRepository;
        private readonly ICurrentUserService currentUserService;
        private readonly IMapper mapper;

        public ReviewService(IReviewRepository reviewRepository, ICurrentUserService currentUserService, IMapper mapper)
        {
            this.reviewRepository = reviewRepository;
            this.currentUserService = currentUserService;
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

        public async Task SetAverageRatingAsync(VenueDto venue)
        {
            venue.Rating = await reviewRepository.GetAverageRatingByVenueIdAsync(venue.Id);
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
            return await reviewRepository.GetSummaryByArtistIdAsync(id);
        }

        public async Task<ReviewSummaryDto> GetSummaryByEventIdAsync(int id)
        {
            return await reviewRepository.GetSummaryByEventIdAsync(id);
        }

        public async Task<ReviewSummaryDto> GetSummaryByVenueIdAsync(int id)
        {
            return await reviewRepository.GetSummaryByVenueIdAsync(id);
        }

        public async Task<bool> CanUserReviewEventIdAsync(int eventId)
        {
            var user = await currentUserService.GetAsync();

            return await reviewRepository.CanUserIdReviewEventIdAsync(user.Id, eventId);
        }

        public async Task<bool> CanUserReviewVenueIdAsync(int venueId)
        {
            var user = await currentUserService.GetAsync();

            return await reviewRepository.CanUserIdReviewVenueIdAsync(user.Id, venueId);
        }

        public async Task<bool> CanUserReviewArtistIdAsync(int artistId)
        {
            var user = await currentUserService.GetAsync();

            return await reviewRepository.CanUserIdReviewArtistIdAsync(user.Id, artistId);
        }
    }
}
