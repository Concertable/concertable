using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Core.Parameters;
using Application.Responses;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    private readonly TimeProvider timeProvider;

    public ReviewRepository(ApplicationDbContext context, TimeProvider timeProvider) : base(context)
    {
        this.timeProvider = timeProvider;
    }

    public Task<ReviewSummaryDto> GetSummaryByArtistIdAsync(int id)
    {
        return GetSummaryAsync(r => r.Ticket.Concert.Application.ArtistId == id);
    }

    public Task<ReviewSummaryDto> GetSummaryByConcertIdAsync(int id)
    {
        return GetSummaryAsync(r => r.Ticket.ConcertId == id);
    }

    public Task<ReviewSummaryDto> GetSummaryByVenueIdAsync(int id)
    {
        return GetSummaryAsync(r => r.Ticket.Concert.Application.Opportunity.VenueId == id);
    }


    public async Task<double> GetAverageRatingByVenueIdAsync(int id)
    {
        var avg = await context.Reviews
            .Where(r => r.Ticket.Concert.Application.Opportunity.VenueId == id)
            .AverageAsync(r => (double?)r.Stars);

        return Math.Round(avg ?? 0.0, 1);
    }


    private async Task<ReviewSummaryDto> GetSummaryAsync(Expression<Func<Review, bool>> filter)
    {
        var query = context.Reviews.Where(filter);

        var averageRating = await query.AverageAsync(r => (double?)r.Stars);
        var totalReviews = await query.CountAsync();

        return new ReviewSummaryDto(
            totalReviews,
            averageRating is double avg ? Math.Round(avg, 1) : null
        );
    }

    private async Task<Pagination<Review>> GetAsync(
        Expression<Func<Review, bool>> predicate,
        IPageParams pageParams)
    {
        var query = context.Reviews
            .Include(r => r.Ticket)
                .ThenInclude(t => t.User)
            .Where(predicate)
            .OrderByDescending(r => r.Id);

        return await PaginationHelper.CreatePaginatedResponseAsync(query, pageParams);
    }

    public Task<Pagination<Review>> GetByConcertIdAsync(int concertId, IPageParams pageParams) =>
        GetAsync(r => r.Ticket.ConcertId == concertId, pageParams);

    public Task<Pagination<Review>> GetByArtistIdAsync(int artistId, IPageParams pageParams) =>
        GetAsync(r => r.Ticket.Concert.Application.ArtistId == artistId, pageParams);

    public Task<Pagination<Review>> GetByVenueIdAsync(int venueId, IPageParams pageParams) =>
        GetAsync(r => r.Ticket.Concert.Application.Opportunity.VenueId == venueId, pageParams);


    private IQueryable<Ticket> GetUnreviewedTicketsByUser(int userId)
    {
        return context.Tickets
            .Where(t => t.UserId == userId && t.Review == null);
    }


    public Task<bool> CanUserIdReviewConcertIdAsync(int userId, int concertId)
    {
        return GetUnreviewedTicketsByUser(userId)
            .AnyAsync(t => t.ConcertId == concertId && t.Concert.Application.Opportunity.StartDate <= timeProvider.GetUtcNow());
    }


    public Task<bool> CanUserIdReviewArtistIdAsync(int userId, int artistId)
    {
        return GetUnreviewedTicketsByUser(userId)
            .AnyAsync(t => t.Concert.Application.Artist.Id == artistId);
    }


    public Task<bool> CanUserIdReviewVenueIdAsync(int userId, int venueId)
    {
        return GetUnreviewedTicketsByUser(userId)
            .AnyAsync(t => t.Concert.Application.Opportunity.Venue.Id == venueId);
    }

}
