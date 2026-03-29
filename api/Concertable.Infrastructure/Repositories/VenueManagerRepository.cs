using Concertable.Application.Interfaces;
using Concertable.Infrastructure.Data;
using Concertable.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories;

public class VenueManagerRepository : GuidRepository<VenueManagerEntity>, IVenueManagerRepository
{
    public VenueManagerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<VenueManagerEntity?> GetByConcertIdAsync(int concertId)
    {
        return await context.Users
            .OfType<VenueManagerEntity>()
            .Where(u => u.Id == context.Concerts
                .Where(c => c.Id == concertId)
                .Select(c => c.Application.Opportunity.Venue.UserId)
                .First())
            .FirstOrDefaultAsync();
    }

    public async Task<VenueManagerEntity?> GetByApplicationIdAsync(int applicationId)
    {
        return await context.Users
            .OfType<VenueManagerEntity>()
            .Where(u => u.Id == context.ConcertApplications
                .Where(a => a.Id == applicationId)
                .Select(a => a.Opportunity.Venue.UserId)
                .First())
            .FirstOrDefaultAsync();
    }
}
