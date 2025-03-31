using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Core.Parameters;
using Application.Responses;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context) { }

        public Task<ReviewSummaryDto> GetSummaryByArtistIdAsync(int id)
        {
            return GetSummaryAsync(r => r.Ticket.Event.Application.ArtistId == id);
        }

        public Task<ReviewSummaryDto> GetSummaryByEventIdAsync(int id)
        {
            return GetSummaryAsync(r => r.Ticket.EventId == id);
        }

        public Task<ReviewSummaryDto> GetSummaryByVenueIdAsync(int id)
        {
            return GetSummaryAsync(r => r.Ticket.Event.Application.Listing.VenueId == id);
        }


        public Task<double?> GetAverageRatingByArtistIdAsync(int id)
        {
            return GetAverageRatingAsync(r => r.Ticket.Event.Application.ArtistId == id);
        }

        public Task<double?> GetAverageRatingByEventIdAsync(int id)
        {
            return GetAverageRatingAsync(r => r.Ticket.EventId == id);
        }

        public Task<double?> GetAverageRatingByVenueIdAsync(int id)
        {
            return GetAverageRatingAsync(r => r.Ticket.Event.Application.Listing.VenueId == id);
        }

        private async Task<double?> GetAverageRatingAsync(Expression<Func<Review, bool>> filter)
        {
            var query = context.Reviews.Where(filter);

            return (await query.AverageAsync(r => (double?)r.Stars)) is double avg
                ? Math.Round(avg, 1)
                : null;
        }

        private async Task<ReviewSummaryDto> GetSummaryAsync(Expression<Func<Review, bool>> filter)
        {
            var query = context.Reviews.Where(filter);

            var averageRating = await query.AverageAsync(r => (double?)r.Stars);
            var totalReviews = await query.CountAsync();

            return new ReviewSummaryDto
            {
                AverageRating = averageRating is double avg ? Math.Round(avg, 1) : null,
                TotalReviews = totalReviews
            };
        }

        private async Task<PaginationResponse<Review>> GetAsync(
            Expression<Func<Review, bool>> predicate,
            PaginationParams pageParams)
        {
            var query = context.Reviews
                .Include(r => r.Ticket)
                    .ThenInclude(t => t.User)
                .Where(predicate)
                .OrderByDescending(r => r.Id); 

            return await PaginationHelper.CreatePaginatedResponseAsync(query, pageParams);
        }

        public Task<PaginationResponse<Review>> GetByEventIdAsync(int eventId, PaginationParams pageParams) =>
            GetAsync(r => r.Ticket.EventId == eventId, pageParams);

        public Task<PaginationResponse<Review>> GetByArtistIdAsync(int artistId, PaginationParams pageParams) =>
            GetAsync(r => r.Ticket.Event.Application.ArtistId == artistId, pageParams);

        public Task<PaginationResponse<Review>> GetByVenueIdAsync(int venueId, PaginationParams pageParams) =>
            GetAsync(r => r.Ticket.Event.Application.Listing.VenueId == venueId, pageParams);


        private IQueryable<Ticket> GetUnreviewedTicketsByUser(int userId)
        {
            return context.Tickets
                .Where(t => t.UserId == userId && t.Review == null);
        }


        public Task<bool> CanUserIdReviewEventIdAsync(int userId, int eventId)
        {
            return GetUnreviewedTicketsByUser(userId)
                .AnyAsync(t => t.EventId == eventId);
        }


        public Task<bool> CanUserIdReviewArtistIdAsync(int userId, int artistId)
        {
            return GetUnreviewedTicketsByUser(userId)
                .AnyAsync(t => t.Event.Application.Artist.Id == artistId);
        }


        public Task<bool> CanUserIdReviewVenueIdAsync(int userId, int venueId)
        {
            return GetUnreviewedTicketsByUser(userId)
                .AnyAsync(t => t.Event.Application.Listing.Venue.Id == venueId);
        }

    }
}
