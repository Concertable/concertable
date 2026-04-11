using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Search;
using Concertable.Core.Entities;
using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Helpers;
using Concertable.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories;

public class ReviewRepository : Repository<ReviewEntity>, IReviewRepository
{
    private readonly TimeProvider timeProvider;
    private readonly IReviewSpecification<ArtistEntity> artistReviewSpecification;
    private readonly IReviewSpecification<VenueEntity> venueReviewSpecification;
    private readonly IReviewSpecification<ConcertEntity> concertReviewSpecification;

    public ReviewRepository(
        ApplicationDbContext context,
        TimeProvider timeProvider,
        IReviewSpecification<ArtistEntity> artistReviewSpecification,
        IReviewSpecification<VenueEntity> venueReviewSpecification,
        IReviewSpecification<ConcertEntity> concertReviewSpecification) : base(context)
    {
        this.timeProvider = timeProvider;
        this.artistReviewSpecification = artistReviewSpecification;
        this.venueReviewSpecification = venueReviewSpecification;
        this.concertReviewSpecification = concertReviewSpecification;
    }

    public Task<ReviewSummaryDto> GetSummaryByArtistIdAsync(int id) =>
        GetSummaryAsync(artistReviewSpecification, id);

    public Task<ReviewSummaryDto> GetSummaryByConcertIdAsync(int id) =>
        GetSummaryAsync(concertReviewSpecification, id);

    public Task<ReviewSummaryDto> GetSummaryByVenueIdAsync(int id) =>
        GetSummaryAsync(venueReviewSpecification, id);

    public Task<IPagination<ReviewDto>> GetByConcertIdAsync(int concertId, IPageParams pageParams) =>
        GetAsync(concertReviewSpecification, concertId, pageParams);

    public Task<IPagination<ReviewDto>> GetByArtistIdAsync(int artistId, IPageParams pageParams) =>
        GetAsync(artistReviewSpecification, artistId, pageParams);

    public Task<IPagination<ReviewDto>> GetByVenueIdAsync(int venueId, IPageParams pageParams) =>
        GetAsync(venueReviewSpecification, venueId, pageParams);

    private async Task<ReviewSummaryDto> GetSummaryAsync<TEntity>(
        IReviewSpecification<TEntity> spec, int id)
        where TEntity : class, IIdEntity, IReviewable<TEntity>
    {
        return await spec.Apply(context.Reviews, id)
            .ToSummaryDto()
            .FirstOrDefaultAsync()
            ?? new ReviewSummaryDto(0, null);
    }

    private Task<IPagination<ReviewDto>> GetAsync<TEntity>(
        IReviewSpecification<TEntity> spec, int id, IPageParams pageParams)
        where TEntity : class, IIdEntity, IReviewable<TEntity>
    {
        return spec.Apply(context.Reviews, id)
            .OrderByDescending(r => r.Id)
            .ToDto()
            .ToPaginationAsync(pageParams);
    }

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
