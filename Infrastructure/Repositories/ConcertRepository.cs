using Core.Entities;
using Application.Interfaces;
using Application.Mappers;
using Infrastructure.Data.Identity;
using Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ConcertRepository : Repository<Concert>, IConcertRepository
{
    private readonly TimeProvider timeProvider;

    public ConcertRepository(ApplicationDbContext context, TimeProvider timeProvider) : base(context)
    {
        this.timeProvider = timeProvider;
    }

    public async Task<Concert?> GetDetailsByIdAsync(int id)
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

    public async Task<IEnumerable<Concert>> GetUpcomingByVenueIdAsync(int id)
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

    public async Task<IEnumerable<Concert>> GetUpcomingByArtistIdAsync(int id)
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

    public async Task<Concert?> GetByApplicationIdAsync(int applicationId)
    {
        return await context.Concerts
            .Where(e => e.ApplicationId == applicationId)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Concert>> GetHistoryByArtistIdAsync(int id)
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

    public async Task<IEnumerable<Concert>> GetHistoryByVenueIdAsync(int id)
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

    public async Task<IEnumerable<Concert>> GetUnpostedByArtistIdAsync(int id)
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

    public async Task<IEnumerable<Concert>> GetUnpostedByVenueIdAsync(int id)
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

    public async Task<decimal?> GetPriceByIdAsync(int id)
    {
        return await context.Concerts
            .Where(e => e.Id == id)
            .Select(e => e.Price)
            .FirstOrDefaultAsync();
    }


}
