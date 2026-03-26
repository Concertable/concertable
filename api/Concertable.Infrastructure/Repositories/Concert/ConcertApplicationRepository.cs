using Core.Entities;
using Application.Interfaces;
using Application.Interfaces.Concert;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;

namespace Infrastructure.Repositories.Concert;

public class ConcertApplicationRepository : Repository<ConcertApplicationEntity>, IConcertApplicationRepository
{
    private readonly TimeProvider timeProvider;

    public ConcertApplicationRepository(ApplicationDbContext context, TimeProvider timeProvider) : base(context)
    {
        this.timeProvider = timeProvider;
    }

    public async Task<IEnumerable<ConcertApplicationEntity>> GetByOpportunityIdAsync(int id)
    {
        var query = context.ConcertApplications
        .Where(ca => ca.OpportunityId == id)
        .Include(ca => ca.Artist)
            .ThenInclude(a => a.User)
        .Include(ca => ca.Artist)
            .ThenInclude(a => a.ArtistGenres)
                .ThenInclude(ag => ag.Genre)
        .Include(ca => ca.Opportunity)
            .ThenInclude(o => o.Contract)
        .Include(ca => ca.Opportunity)
            .ThenInclude(o => o.OpportunityGenres)
                .ThenInclude(og => og.Genre);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<ConcertApplicationEntity>> GetPendingByArtistIdAsync(int artistId)
    {
        var query = context.ConcertApplications
        .Include(a => a.Opportunity)
            .ThenInclude(o => o.Venue)
        .Where(a =>
            a.ArtistId == artistId &&
            !context.Concerts.Any(e => e.ApplicationId == a.Id) &&
            a.Opportunity.StartDate > timeProvider.GetUtcNow());

        return await query.ToListAsync();
    }

    public async Task<(ArtistEntity, VenueEntity)?> GetArtistAndVenueByIdAsync(int id)
    {
        var query = await context.ConcertApplications
            .Where(ca => ca.Id == id)
            .Include(ca => ca.Artist)
                .ThenInclude(a => a.User)
            .Include(ca => ca.Artist)
                .ThenInclude(a => a.ArtistGenres)
            .Include(ca => ca.Opportunity)
                .ThenInclude(o => o.Venue)
                    .ThenInclude(v => v.User)
            .Include(ca => ca.Opportunity.OpportunityGenres)
            .FirstOrDefaultAsync();

        if (query is null) return null;
        return (query.Artist, query.Opportunity.Venue);
    }

    public async new Task<ConcertApplicationEntity?> GetByIdAsync(int id)
    {
        var query = context.ConcertApplications
            .Where(ca => ca.Id == id)
            .Include(ca => ca.Artist)
                .ThenInclude(ca => ca.ArtistGenres)
                    .ThenInclude(ca => ca.Genre)
            .Include(ca => ca.Artist.User)
            .Include(ca => ca.Opportunity)
                .ThenInclude(o => o.Contract)
            .Include(ca => ca.Opportunity)
                .ThenInclude(o => o.OpportunityGenres)
                    .ThenInclude(og => og.Genre);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<ConcertApplicationEntity?> GetByConcertIdAsync(int concertId)
    {
        return await context.Concerts
            .Where(c => c.Id == concertId)
            .Select(c => c.Application)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ConcertApplicationEntity>> GetRecentDeniedByArtistIdAsync(int artistId)
    {
        var query = context.ConcertApplications
            .Include(a => a.Opportunity)
                .ThenInclude(o => o.Venue)
            .Where(a =>
                a.ArtistId == artistId &&
                context.Concerts.Any(e =>
                    e.Application.OpportunityId == a.OpportunityId &&
                    e.ApplicationId != a.Id)) // someone else was accepted
            .OrderByDescending(a => a.Opportunity.EndDate)
            .Take(5);

        return await query.ToListAsync();
    }
}
