using Concertable.Data.Application;
using Concertable.Data.Infrastructure;
using Concertable.Identity.Application.Interfaces;
using Concertable.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Identity.Infrastructure.Repositories;

internal class VenueManagerRepository(IdentityDbContext context, IReadDbContext readDb)
    : GuidModuleRepository<VenueManagerEntity, IdentityDbContext>(context),
      IManagerRepository<VenueManagerEntity>
{
    public async Task<VenueManagerEntity?> GetByConcertIdAsync(int concertId)
    {
        var userId = await readDb.Concerts
            .Where(c => c.Id == concertId)
            .Select(c => (Guid?)c.Booking.Application.Opportunity.Venue.UserId)
            .FirstOrDefaultAsync();
        return userId.HasValue
            ? await context.Users.OfType<VenueManagerEntity>().FirstOrDefaultAsync(u => u.Id == userId)
            : null;
    }

    public async Task<VenueManagerEntity?> GetByApplicationIdAsync(int applicationId)
    {
        var userId = await readDb.OpportunityApplications
            .Where(a => a.Id == applicationId)
            .Select(a => (Guid?)a.Opportunity.Venue.UserId)
            .FirstOrDefaultAsync();
        return userId.HasValue
            ? await context.Users.OfType<VenueManagerEntity>().FirstOrDefaultAsync(u => u.Id == userId)
            : null;
    }
}
