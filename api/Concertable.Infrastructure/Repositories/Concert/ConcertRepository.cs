using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Search;
using Concertable.Core.Entities;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Mappers;
using Concertable.Core.Extensions;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;
using Concertable.Infrastructure.Data;

namespace Concertable.Infrastructure.Repositories.Concert;

public class ConcertRepository : Repository<Core.Entities.ConcertEntity>, IConcertRepository
{
    private readonly TimeProvider timeProvider;
    private readonly IRatingSpecification<ConcertEntity> concertRatingSpecification;
    private readonly IRatingSpecification<ArtistEntity> artistRatingSpecification;
    private readonly IRatingSpecification<VenueEntity> venueRatingSpecification;

    public ConcertRepository(
        ApplicationDbContext context,
        TimeProvider timeProvider,
        IRatingSpecification<ConcertEntity> concertRatingSpecification,
        IRatingSpecification<ArtistEntity> artistRatingSpecification,
        IRatingSpecification<VenueEntity> venueRatingSpecification) : base(context)
    {
        this.timeProvider = timeProvider;
        this.concertRatingSpecification = concertRatingSpecification;
        this.artistRatingSpecification = artistRatingSpecification;
        this.venueRatingSpecification = venueRatingSpecification;
    }

    public async Task<ConcertEntity?> GetAggregateByIdAsync(int id)
    {
        return await context.Concerts
            .Where(e => e.Id == id)
            .Include(e => e.Application)
                .ThenInclude(ca => ca.Artist)
                    .ThenInclude(a => a.User)
            .Include(e => e.Application)
                .ThenInclude(ca => ca.Artist)
                    .ThenInclude(a => a.ArtistGenres)
                        .ThenInclude(ag => ag.Genre)
            .Include(e => e.Application.Opportunity)
                .ThenInclude(o => o.Venue)
                    .ThenInclude(v => v.User)
            .Include(e => e.ConcertGenres)
                .ThenInclude(eg => eg.Genre)
            .FirstOrDefaultAsync();
    }

    public async Task<ConcertSummaryDto?> GetSummaryAsync(int id)
    {
        var artistRatings = artistRatingSpecification.ApplyAggregate(context.Reviews);
        var venueRatings = venueRatingSpecification.ApplyAggregate(context.Reviews);
        return await context.Concerts
            .Where(e => e.Id == id)
            .ToSummaryDto(artistRatings, venueRatings)
            .FirstOrDefaultAsync();
    }

    public async Task<ConcertDto?> GetDtoByIdAsync(int id)
    {
        var concertRatings = concertRatingSpecification.ApplyAggregate(context.Reviews);
        var artistRatings = artistRatingSpecification.ApplyAggregate(context.Reviews);
        return await context.Concerts
            .Where(e => e.Id == id)
            .ToDto(concertRatings, artistRatings)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ConcertSummaryDto>> GetUpcomingByVenueIdAsync(int id)
    {
        var artistRatings = artistRatingSpecification.ApplyAggregate(context.Reviews);
        var venueRatings = venueRatingSpecification.ApplyAggregate(context.Reviews);
        return await context.Concerts
            .Where(e => e.Application.Opportunity.VenueId == id
                        && e.Application.Opportunity.StartDate >= timeProvider.GetUtcNow()
                        && e.DatePosted != null)
            .ToSummaryDto(artistRatings, venueRatings)
            .ToListAsync();
    }

    public async Task<IEnumerable<ConcertSummaryDto>> GetUpcomingByArtistIdAsync(int id)
    {
        var artistRatings = artistRatingSpecification.ApplyAggregate(context.Reviews);
        var venueRatings = venueRatingSpecification.ApplyAggregate(context.Reviews);
        return await context.Concerts
            .Where(e => e.Application.ArtistId == id
                        && e.Application.Opportunity.StartDate >= timeProvider.GetUtcNow()
                        && e.DatePosted != null)
            .ToSummaryDto(artistRatings, venueRatings)
            .ToListAsync();
    }

    public async Task<ConcertDto?> GetDtoByApplicationIdAsync(int applicationId)
    {
        var concertRatings = concertRatingSpecification.ApplyAggregate(context.Reviews);
        var artistRatings = artistRatingSpecification.ApplyAggregate(context.Reviews);
        return await context.Concerts
            .Where(e => e.ApplicationId == applicationId)
            .ToDto(concertRatings, artistRatings)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ConcertSummaryDto>> GetHistoryByArtistIdAsync(int id)
    {
        var artistRatings = artistRatingSpecification.ApplyAggregate(context.Reviews);
        var venueRatings = venueRatingSpecification.ApplyAggregate(context.Reviews);
        return await context.Concerts
            .Where(e => e.Application.ArtistId == id
                        && e.Application.Opportunity.StartDate < timeProvider.GetUtcNow()
                        && e.DatePosted != null)
            .ToSummaryDto(artistRatings, venueRatings)
            .ToListAsync();
    }

    public async Task<IEnumerable<ConcertSummaryDto>> GetHistoryByVenueIdAsync(int id)
    {
        var artistRatings = artistRatingSpecification.ApplyAggregate(context.Reviews);
        var venueRatings = venueRatingSpecification.ApplyAggregate(context.Reviews);
        return await context.Concerts
            .Where(e => e.Application.Opportunity.VenueId == id
                        && e.Application.Opportunity.StartDate < timeProvider.GetUtcNow()
                        && e.DatePosted != null)
            .ToSummaryDto(artistRatings, venueRatings)
            .ToListAsync();
    }

    public async Task<IEnumerable<ConcertSummaryDto>> GetUnpostedByArtistIdAsync(int id)
    {
        var artistRatings = artistRatingSpecification.ApplyAggregate(context.Reviews);
        var venueRatings = venueRatingSpecification.ApplyAggregate(context.Reviews);
        return await context.Concerts
            .Where(e => e.Application.ArtistId == id && e.DatePosted == null)
            .ToSummaryDto(artistRatings, venueRatings)
            .ToListAsync();
    }

    public async Task<IEnumerable<ConcertSummaryDto>> GetUnpostedByVenueIdAsync(int id)
    {
        var artistRatings = artistRatingSpecification.ApplyAggregate(context.Reviews);
        var venueRatings = venueRatingSpecification.ApplyAggregate(context.Reviews);
        return await context.Concerts
            .Where(e => e.Application.Opportunity.VenueId == id && e.DatePosted == null)
            .ToSummaryDto(artistRatings, venueRatings)
            .ToListAsync();
    }

    public async Task<bool> ArtistHasConcertOnDateAsync(int artistId, DateTime date)
    {
        return await context.Concerts
            .Where(e => e.Application.ArtistId == artistId)
            .AnyAsync(e => e.Application.Opportunity.StartDate.Date == date.Date);
    }

    public Task<bool> OpportunityHasConcertAsync(int opportunityId)
    {
        return context.Concerts.AnyAsync(e => e.Application.OpportunityId == opportunityId);
    }

    public async Task<bool> VenueHasConcertOnDateAsync(int venueId, DateTime date)
    {
        return await context.Concerts
            .Where(e => e.Application.Opportunity.VenueId == venueId)
            .AnyAsync(e => e.Application.Opportunity.StartDate.Date == date.Date);
    }

    public async Task<ContractType?> GetTypeByIdAsync(int id)
    {
        var contract = await context.Concerts
            .Where(c => c.Id == id)
            .Select(c => c.Application.Opportunity.Contract)
            .FirstOrDefaultAsync();

        return contract?.ContractType;
    }

    public async Task<IEnumerable<int>> GetEndedConfirmedIdsAsync()
    {
        return await context.Concerts
            .Where(c => c.Application.Status == ApplicationStatus.Confirmed
                     && c.Application.Opportunity.StartDate < timeProvider.GetUtcNow())
            .Select(c => c.Id)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalRevenueByConcertIdAsync(int concertId)
    {
        return await context.Concerts
            .Where(c => c.Id == concertId)
            .Select(c => c.Price * (c.TotalTickets - c.AvailableTickets))
            .FirstOrDefaultAsync();
    }
}
