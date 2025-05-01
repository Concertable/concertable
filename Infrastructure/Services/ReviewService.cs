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
using Core.Interfaces;
using Application.Requests;
using Core.Exceptions;

namespace Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository reviewRepository;
        private readonly ITicketRepository ticketRepository;
        private readonly ICurrentUserService currentUserService;
        private readonly IMapper mapper;

        public ReviewService(
            IReviewRepository reviewRepository, 
            ITicketRepository ticketRepository,
            ICurrentUserService currentUserService, 
            IMapper mapper)
        {
            this.reviewRepository = reviewRepository;
            this.ticketRepository = ticketRepository;
            this.currentUserService = currentUserService;
            this.mapper = mapper;
        }

        public Task AddAverageRatingsAsync(IEnumerable<ArtistHeaderDto> headers)
        {
            return AddAverageRatingsAsync(headers, reviewRepository.GetAverageRatingsByArtistIdsAsync);
        }

        public Task AddAverageRatingsAsync(IEnumerable<EventHeaderDto> headers)
        {
            return AddAverageRatingsAsync(headers, reviewRepository.GetAverageRatingsByEventIdsAsync);
        }

        public Task AddAverageRatingsAsync(IEnumerable<VenueHeaderDto> headers)
        {
            return AddAverageRatingsAsync(headers, reviewRepository.GetAverageRatingsByVenueIdsAsync);
        }

        private async Task AddAverageRatingsAsync<THeader>(
            IEnumerable<THeader> headers,
            Func<IEnumerable<int>, Task<IDictionary<int, double>>> getRatingsAsync)
            where THeader : HeaderDto
        {
            var ids = headers.Select(h => h.Id).ToList();
            var ratings = await getRatingsAsync(ids);

            foreach (var h in headers)
                h.Rating = ratings.TryGetValue(h.Id, out var rating) ? rating : 0.0;
        }


        public async Task SetAverageRatingAsync(VenueDto venue)
        {
            if (venue is not null)
                venue.Rating = await reviewRepository.GetAverageRatingByVenueIdAsync(venue.Id);
        }

        public async Task<ReviewDto> CreateAsync(CreateReviewRequest request)
        {
            var review = mapper.Map<Review>(request);

            var userId = await currentUserService.GetIdAsync();

            var ticket = await ticketRepository.GetByUserIdAndEventIdAsync(userId, request.EventId);

            if (ticket is null)
                throw new NotFoundException("Cannot find ticket");

            review.TicketId = ticket.Id;

            await reviewRepository.AddAsync(review);
            await reviewRepository.SaveChangesAsync();

            return mapper.Map<ReviewDto>(review);
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
