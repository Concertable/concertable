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

    public ConcertRepository(
        ApplicationDbContext context,
        TimeProvider timeProvider,
        IRatingSpecification<ConcertEntity> concertRatingSpecification,
        IRatingSpecification<ArtistEntity> artistRatingSpecification) : base(context)
    {
        this.timeProvider = timeProvider;
        this.concertRatingSpecification = concertRatingSpecification;
        this.artistRatingSpecification = artistRatingSpecification;
    }

    public new async Task<ConcertEntity?> GetByIdAsync(int id)
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

    public async Task<ConcertDto?> GetDetailsByIdAsync(int id)
    {
        var concertRatings = concertRatingSpecification.ApplyAggregate(context.Reviews);
        var artistRatings = artistRatingSpecification.ApplyAggregate(context.Reviews);
        return await context.Concerts
            .Where(e => e.Id == id)
            .ToDto(concertRatings, artistRatings)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ConcertEntity>> GetUpcomingByVenueIdAsync(int id)
    {
        return await context.Concerts
            .Where(e => e.Application.Opportunity.VenueId == id
                        && e.Application.Opportunity.StartDate >= timeProvider.GetUtcNow()
                        && e.DatePosted != null)
            .Include(e => e.Application)
                .ThenInclude(a => a.Opportunity)
            .Include(e => e.Application.Opportunity.Venue)
                .ThenInclude(v => v.User)
            .Include(e => e.Application.Artist)
                .ThenInclude(a => a.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<ConcertEntity>> GetUpcomingByArtistIdAsync(int id)
    {
        return await context.Concerts
            .Where(e => e.Application.ArtistId == id
                        && e.Application.Opportunity.StartDate >= timeProvider.GetUtcNow()
                        && e.DatePosted != null)
            .Include(e => e.Application)
                .ThenInclude(a => a.Opportunity)
            .Include(e => e.Application.Opportunity.Venue)
                .ThenInclude(v => v.User)
            .Include(e => e.Application.Artist)
                .ThenInclude(a => a.User)
            .ToListAsync();
    }

    public async Task<ConcertDto?> GetDetailsByApplicationIdAsync(int applicationId)
    {
        var concertRatings = concertRatingSpecification.ApplyAggregate(context.Reviews);
        var artistRatings = artistRatingSpecification.ApplyAggregate(context.Reviews);
        return await context.Concerts
            .Where(e => e.ApplicationId == applicationId)
            .ToDto(concertRatings, artistRatings)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ConcertEntity>> GetHistoryByArtistIdAsync(int id)
    {
        return await context.Concerts
            .Where(e => e.Application.ArtistId == id
                        && e.Application.Opportunity.StartDate < timeProvider.GetUtcNow()
                        && e.DatePosted != null)
            .Include(e => e.Application)
                .ThenInclude(a => a.Opportunity)
            .Include(e => e.Application.Opportunity.Venue)
                .ThenInclude(v => v.User)
            .Include(e => e.Application.Artist)
                .ThenInclude(a => a.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<ConcertEntity>> GetHistoryByVenueIdAsync(int id)
    {
        return await context.Concerts
            .Where(e => e.Application.Opportunity.VenueId == id
                        && e.Application.Opportunity.StartDate < timeProvider.GetUtcNow()
                        && e.DatePosted != null)
            .Include(e => e.Application)
                .ThenInclude(a => a.Opportunity)
            .Include(e => e.Application.Opportunity.Venue)
                .ThenInclude(v => v.User)
            .Include(e => e.Application.Artist)
                .ThenInclude(a => a.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<ConcertEntity>> GetUnpostedByArtistIdAsync(int id)
    {
        return await context.Concerts
            .Where(e => e.Application.ArtistId == id && e.DatePosted == null)
            .Include(e => e.Application.Opportunity)
            .Include(e => e.Application.Opportunity.Venue)
                .ThenInclude(v => v.User)
            .Include(e => e.Application.Artist)
                .ThenInclude(a => a.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<ConcertEntity>> GetUnpostedByVenueIdAsync(int id)
    {
        return await context.Concerts
            .Where(e => e.Application.Opportunity.VenueId == id && e.DatePosted == null)
            .Include(e => e.Application.Opportunity)
            .Include(e => e.Application.Opportunity.Venue)
                .ThenInclude(v => v.User)
            .Include(e => e.Application.Artist)
                .ThenInclude(a => a.User)
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
