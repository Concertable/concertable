using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Mappers;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;
using Concertable.Infrastructure.Data;
using Concertable.Search.Application.Interfaces;

namespace Concertable.Infrastructure.Repositories.Concert;

internal class ConcertRepository : Repository<ConcertEntity>, IConcertRepository
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

    public async Task<ConcertEntity?> GetFullByIdAsync(int id)
    {
        return await context.Concerts
            .Where(e => e.Id == id)
            .Include(e => e.Booking)
                .ThenInclude(b => b.Application)
                    .ThenInclude(ca => ca.Artist)
                        .ThenInclude(a => a.Genres)
                            .ThenInclude(g => g.Genre)
            .Include(e => e.Booking)
                .ThenInclude(b => b.Application)
                    .ThenInclude(a => a.Opportunity)
                .ThenInclude(o => o.Venue)
            .Include(e => e.ConcertGenres)
                .ThenInclude(eg => eg.Genre)
            .FirstOrDefaultAsync();
    }

    public async Task<ConcertSummaryDto?> GetSummaryAsync(int id)
    {
        return await context.Concerts
            .Where(e => e.Id == id)
            .ToSummaryDto(
                artistRatingSpecification.ApplyAggregate(context.Reviews),
                venueRatingSpecification.ApplyAggregate(context.Reviews))
            .FirstOrDefaultAsync();
    }

    public async Task<ConcertDto?> GetDtoByIdAsync(int id)
    {
        return await context.Concerts
            .Where(e => e.Id == id)
            .ToDto(
                concertRatingSpecification.ApplyAggregate(context.Reviews),
                artistRatingSpecification.ApplyAggregate(context.Reviews),
                venueRatingSpecification.ApplyAggregate(context.Reviews))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ConcertSummaryDto>> GetUpcomingByVenueIdAsync(int id)
    {
        return await context.Concerts
            .Where(e => e.Booking.Application.Opportunity.VenueId == id
                        && e.Booking.Application.Opportunity.Period.Start >= timeProvider.GetUtcNow()
                        && e.DatePosted != null)
            .ToSummaryDto(
                artistRatingSpecification.ApplyAggregate(context.Reviews),
                venueRatingSpecification.ApplyAggregate(context.Reviews))
            .ToListAsync();
    }

    public async Task<IEnumerable<ConcertSummaryDto>> GetUpcomingByArtistIdAsync(int id)
    {
        return await context.Concerts
            .Where(e => e.Booking.Application.ArtistId == id
                        && e.Booking.Application.Opportunity.Period.Start >= timeProvider.GetUtcNow()
                        && e.DatePosted != null)
            .ToSummaryDto(
                artistRatingSpecification.ApplyAggregate(context.Reviews),
                venueRatingSpecification.ApplyAggregate(context.Reviews))
            .ToListAsync();
    }

    public async Task<ConcertDto?> GetDtoByApplicationIdAsync(int applicationId)
    {
        return await context.Concerts
            .Where(e => e.Booking.ApplicationId == applicationId)
            .ToDto(
                concertRatingSpecification.ApplyAggregate(context.Reviews),
                artistRatingSpecification.ApplyAggregate(context.Reviews),
                venueRatingSpecification.ApplyAggregate(context.Reviews))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ConcertSummaryDto>> GetHistoryByArtistIdAsync(int id)
    {
        return await context.Concerts
            .Where(e => e.Booking.Application.ArtistId == id
                        && e.Booking.Application.Opportunity.Period.Start < timeProvider.GetUtcNow()
                        && e.DatePosted != null)
            .ToSummaryDto(
                artistRatingSpecification.ApplyAggregate(context.Reviews),
                venueRatingSpecification.ApplyAggregate(context.Reviews))
            .ToListAsync();
    }

    public async Task<IEnumerable<ConcertSummaryDto>> GetHistoryByVenueIdAsync(int id)
    {
        return await context.Concerts
            .Where(e => e.Booking.Application.Opportunity.VenueId == id
                        && e.Booking.Application.Opportunity.Period.Start < timeProvider.GetUtcNow()
                        && e.DatePosted != null)
            .ToSummaryDto(
                artistRatingSpecification.ApplyAggregate(context.Reviews),
                venueRatingSpecification.ApplyAggregate(context.Reviews))
            .ToListAsync();
    }

    public async Task<IEnumerable<ConcertSummaryDto>> GetUnpostedByArtistIdAsync(int id)
    {
        return await context.Concerts
            .Where(e => e.Booking.Application.ArtistId == id && e.DatePosted == null)
            .ToSummaryDto(
                artistRatingSpecification.ApplyAggregate(context.Reviews),
                venueRatingSpecification.ApplyAggregate(context.Reviews))
            .ToListAsync();
    }

    public async Task<IEnumerable<ConcertSummaryDto>> GetUnpostedByVenueIdAsync(int id)
    {
        return await context.Concerts
            .Where(e => e.Booking.Application.Opportunity.VenueId == id && e.DatePosted == null)
            .ToSummaryDto(
                artistRatingSpecification.ApplyAggregate(context.Reviews),
                venueRatingSpecification.ApplyAggregate(context.Reviews))
            .ToListAsync();
    }

    public async Task<bool> ArtistHasConcertOnDateAsync(int artistId, DateTime date)
    {
        return await context.Concerts
            .Where(e => e.Booking.Application.ArtistId == artistId)
            .AnyAsync(e => e.Booking.Application.Opportunity.Period.Start.Date == date.Date);
    }

    public Task<bool> OpportunityHasConcertAsync(int opportunityId)
    {
        return context.Concerts.AnyAsync(e => e.Booking.Application.OpportunityId == opportunityId);
    }

    public async Task<bool> VenueHasConcertOnDateAsync(int venueId, DateTime date)
    {
        return await context.Concerts
            .Where(e => e.Booking.Application.Opportunity.VenueId == venueId)
            .AnyAsync(e => e.Booking.Application.Opportunity.Period.Start.Date == date.Date);
    }

    public async Task<ContractType?> GetTypeByIdAsync(int id)
    {
        var contract = await context.Concerts
            .Where(c => c.Id == id)
            .Select(c => c.Booking.Application.Opportunity.Contract)
            .FirstOrDefaultAsync();

        return contract?.ContractType;
    }

    public async Task<IEnumerable<int>> GetEndedConfirmedIdsAsync()
    {
        return await context.Concerts
            .Where(c => c.Booking.Application.Status == ApplicationStatus.Accepted
                     && c.Booking.Application.Opportunity.Period.Start < timeProvider.GetUtcNow())
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
