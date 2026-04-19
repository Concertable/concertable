using Concertable.Core.Entities;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using Concertable.Infrastructure.Data;

namespace Concertable.Infrastructure.Repositories.Concert;

public class OpportunityApplicationRepository : Repository<OpportunityApplicationEntity>, IOpportunityApplicationRepository
{
    private readonly TimeProvider timeProvider;

    public OpportunityApplicationRepository(ApplicationDbContext context, TimeProvider timeProvider) : base(context)
    {
        this.timeProvider = timeProvider;
    }

    public async Task<IEnumerable<OpportunityApplicationEntity>> GetByOpportunityIdAsync(int id)
    {
        var query = context.OpportunityApplications
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

    public async Task<IEnumerable<OpportunityApplicationEntity>> GetPendingByArtistIdAsync(int artistId)
    {
        var query = context.OpportunityApplications
        .Include(a => a.Opportunity)
            .ThenInclude(o => o.Venue)
        .Where(a =>
            a.ArtistId == artistId &&
            !context.Concerts.Any(e => e.ApplicationId == a.Id) &&
            a.Opportunity.Period.Start > timeProvider.GetUtcNow());

        return await query.ToListAsync();
    }

    public async Task<(ArtistEntity, VenueEntity)?> GetArtistAndVenueByIdAsync(int id)
    {
        var query = await context.OpportunityApplications
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

    public async new Task<OpportunityApplicationEntity?> GetByIdAsync(int id)
    {
        var query = context.OpportunityApplications
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

    public async Task<OpportunityApplicationEntity?> GetByConcertIdAsync(int concertId)
    {
        return await context.OpportunityApplications
            .Where(a => a.Concert!.Id == concertId)
            .Include(a => a.Opportunity)
            .FirstOrDefaultAsync();
    }

    public async Task RejectAllExceptAsync(int opportunityId, int applicationId)
    {
        await context.OpportunityApplications
            .Where(a => a.OpportunityId == opportunityId && a.Id != applicationId && a.Status == ApplicationStatus.Pending)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.Status, ApplicationStatus.Rejected));
    }

    public async Task<IEnumerable<OpportunityApplicationEntity>> GetRecentDeniedByArtistIdAsync(int artistId)
    {
        var query = context.OpportunityApplications
            .Include(a => a.Opportunity)
                .ThenInclude(o => o.Venue)
            .Where(a =>
                a.ArtistId == artistId &&
                context.Concerts.Any(e =>
                    e.Application.OpportunityId == a.OpportunityId &&
                    e.ApplicationId != a.Id)) // someone else was accepted
            .OrderByDescending(a => a.Opportunity.Period.End)
            .Take(5);

        return await query.ToListAsync();
    }
}
