using Core.Interfaces;
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
        return GetSummaryAsync(r => r.Ticket.Concert.Application.Listing.VenueId == id);
    }


    public Task<double> GetAverageRatingByArtistIdAsync(int id)
    {
        return GetAverageRatingAsync(r => r.Ticket.Concert.Application.ArtistId == id);
    }

    public Task<double> GetAverageRatingByConcertIdAsync(int id)
    {
        return GetAverageRatingAsync(r => r.Ticket.ConcertId == id);
    }

    public Task<double> GetAverageRatingByVenueIdAsync(int id)
    {
        return GetAverageRatingAsync(r => r.Ticket.Concert.Application.Listing.VenueId == id);
    }

    private async Task<double> GetAverageRatingAsync(Expression<Func<Review, bool>> filter)
    {
        var query = context.Reviews.Where(filter);

        return Math.Round((await query.AverageAsync(r => (double?)r.Stars)) ?? 0.0, 1);
    }

    /// <summary>
    /// Creates the full expression tree through the passing of specific expressions
    /// from other function calls
    /// </summary>
    private async Task<IDictionary<int, double>> GetAverageRatingsAsync(
        IEnumerable<int> ids,
        Expression<Func<Review, int>> keySelector,
        Expression<Func<Review, bool>> idFilter)
    {
        if (!ids.Any())
            return new Dictionary<int, double>();

        var query = context.Reviews.AsQueryable();

        query = query.Where(idFilter);

        return await query
            .GroupBy(keySelector)
            .Select(g => new
            {
                Id = g.Key,
                AvgRating = g.Average(r => (double?)r.Stars) ?? 0.0
            })
            .ToDictionaryAsync(
                x => x.Id,
                x => Math.Round(x.AvgRating, 1)
            );
    }

    public Task<IDictionary<int, double>> GetAverageRatingsByArtistIdsAsync(IEnumerable<int> ids)
    {
        return GetAverageRatingsAsync(
            ids,
            r => r.Ticket.Concert.Application.ArtistId,
            r => ids.Contains(r.Ticket.Concert.Application.ArtistId));
    }

    public Task<IDictionary<int, double>> GetAverageRatingsByConcertIdsAsync(IEnumerable<int> ids)
    {
        return GetAverageRatingsAsync(
            ids,
            r => r.Ticket.ConcertId,
            r => ids.Contains(r.Ticket.ConcertId));
    }

    public Task<IDictionary<int, double>> GetAverageRatingsByVenueIdsAsync(IEnumerable<int> ids)
    {
        return GetAverageRatingsAsync(
            ids,
            r => r.Ticket.Concert.Application.Listing.VenueId,
            r => ids.Contains(r.Ticket.Concert.Application.Listing.VenueId));
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
        GetAsync(r => r.Ticket.Concert.Application.Listing.VenueId == venueId, pageParams);


    private IQueryable<Ticket> GetUnreviewedTicketsByUser(int userId)
    {
        return context.Tickets
            .Where(t => t.UserId == userId && t.Review == null);
    }


    public Task<bool> CanUserIdReviewConcertIdAsync(int userId, int concertId)
    {
        return GetUnreviewedTicketsByUser(userId)
            .AnyAsync(t => t.ConcertId == concertId && t.Concert.Application.Listing.StartDate <= timeProvider.GetUtcNow());
    }


    public Task<bool> CanUserIdReviewArtistIdAsync(int userId, int artistId)
    {
        return GetUnreviewedTicketsByUser(userId)
            .AnyAsync(t => t.Concert.Application.Artist.Id == artistId);
    }


    public Task<bool> CanUserIdReviewVenueIdAsync(int userId, int venueId)
    {
        return GetUnreviewedTicketsByUser(userId)
            .AnyAsync(t => t.Concert.Application.Listing.Venue.Id == venueId);
    }

}
