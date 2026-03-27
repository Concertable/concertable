using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Core.Parameters;
using Application.Responses;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Core.Interfaces;
using Concertable.Infrastructure.Data;

namespace Infrastructure.Repositories;

public class ReviewRepository : Repository<ReviewEntity>, IReviewRepository
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


    private async Task<ReviewSummaryDto> GetSummaryAsync(Expression<Func<ReviewEntity, bool>> filter)
    {
        var result = await context.Reviews
            .Where(filter)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                AverageRating = g.Average(r => (double?)r.Stars),
                TotalReviews = g.Count()
            })
            .FirstOrDefaultAsync();

        return new ReviewSummaryDto(
            result?.TotalReviews ?? 0,
            result?.AverageRating is double avg ? Math.Round(avg, 1) : null
        );
    }

    private async Task<Pagination<ReviewEntity>> GetAsync(
        Expression<Func<ReviewEntity, bool>> predicate,
        IPageParams pageParams)
    {
        var query = context.Reviews
            .Include(r => r.Ticket)
                .ThenInclude(t => t.User)
            .Where(predicate)
            .OrderByDescending(r => r.Id);

        return await PaginationHelper.CreatePaginatedResponseAsync(query, pageParams);
    }

    public Task<Pagination<ReviewEntity>> GetByConcertIdAsync(int concertId, IPageParams pageParams) =>
        GetAsync(r => r.Ticket.ConcertId == concertId, pageParams);

    public Task<Pagination<ReviewEntity>> GetByArtistIdAsync(int artistId, IPageParams pageParams) =>
        GetAsync(r => r.Ticket.Concert.Application.ArtistId == artistId, pageParams);

    public Task<Pagination<ReviewEntity>> GetByVenueIdAsync(int venueId, IPageParams pageParams) =>
        GetAsync(r => r.Ticket.Concert.Application.Opportunity.VenueId == venueId, pageParams);


    private IQueryable<TicketEntity> GetUnreviewedTicketsByUser(Guid userId)
    {
        return context.Tickets
            .Where(t => t.UserId == userId && t.Review == null);
    }


    public Task<bool> CanUserIdReviewConcertIdAsync(Guid userId, int concertId)
    {
        return GetUnreviewedTicketsByUser(userId)
            .AnyAsync(t => t.ConcertId == concertId && t.Concert.Application.Opportunity.StartDate <= timeProvider.GetUtcNow());
    }


    public Task<bool> CanUserIdReviewArtistIdAsync(Guid userId, int artistId)
    {
        return GetUnreviewedTicketsByUser(userId)
            .AnyAsync(t => t.Concert.Application.Artist.Id == artistId);
    }


    public Task<bool> CanUserIdReviewVenueIdAsync(Guid userId, int venueId)
    {
        return GetUnreviewedTicketsByUser(userId)
            .AnyAsync(t => t.Concert.Application.Opportunity.Venue.Id == venueId);
    }

}
