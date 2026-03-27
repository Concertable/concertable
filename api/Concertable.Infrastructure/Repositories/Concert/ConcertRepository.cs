using Core.Entities;
using Application.Interfaces;
using Application.Interfaces.Concert;
using Application.Mappers;
using Core.Extensions;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using Concertable.Infrastructure.Data;

namespace Infrastructure.Repositories.Concert;

public class ConcertRepository : Repository<Core.Entities.ConcertEntity>, IConcertRepository
{
    private readonly TimeProvider timeProvider;

    public ConcertRepository(ApplicationDbContext context, TimeProvider timeProvider) : base(context)
    {
        this.timeProvider = timeProvider;
    }

    public async Task<Core.Entities.ConcertEntity?> GetDetailsByIdAsync(int id)
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

    public async Task<IEnumerable<Core.Entities.ConcertEntity>> GetUpcomingByVenueIdAsync(int id)
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

    public async Task<IEnumerable<Core.Entities.ConcertEntity>> GetUpcomingByArtistIdAsync(int id)
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

    public async Task<Core.Entities.ConcertEntity?> GetByApplicationIdAsync(int applicationId)
    {
        return await context.Concerts
            .Where(e => e.ApplicationId == applicationId)
            .Include(e => e.Application)
                .ThenInclude(a => a.Opportunity)
                    .ThenInclude(o => o.Venue)
                        .ThenInclude(v => v.User)
            .Include(e => e.Application)
                .ThenInclude(a => a.Artist)
                    .ThenInclude(a => a.User)
            .Include(e => e.ConcertGenres)
                .ThenInclude(cg => cg.Genre)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Core.Entities.ConcertEntity>> GetHistoryByArtistIdAsync(int id)
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

    public async Task<IEnumerable<Core.Entities.ConcertEntity>> GetHistoryByVenueIdAsync(int id)
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

    public async Task<IEnumerable<Core.Entities.ConcertEntity>> GetUnpostedByArtistIdAsync(int id)
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

    public async Task<IEnumerable<Core.Entities.ConcertEntity>> GetUnpostedByVenueIdAsync(int id)
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
