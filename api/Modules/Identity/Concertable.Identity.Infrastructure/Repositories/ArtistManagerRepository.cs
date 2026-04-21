using Concertable.Data.Application;
using Concertable.Data.Infrastructure;
using Concertable.Identity.Application.Interfaces;
using Concertable.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Identity.Infrastructure.Repositories;

internal class ArtistManagerRepository(IdentityDbContext context, IReadDbContext readDb)
    : GuidModuleRepository<ArtistManagerEntity, IdentityDbContext>(context),
      IManagerRepository<ArtistManagerEntity>
{
    public async Task<ArtistManagerEntity?> GetByConcertIdAsync(int concertId)
    {
        var userId = await readDb.Concerts
            .Where(c => c.Id == concertId)
            .Select(c => (Guid?)c.Booking.Application.Artist.UserId)
            .FirstOrDefaultAsync();
        return userId.HasValue
            ? await context.Users.OfType<ArtistManagerEntity>().FirstOrDefaultAsync(u => u.Id == userId)
            : null;
    }

    public async Task<ArtistManagerEntity?> GetByApplicationIdAsync(int applicationId)
    {
        var userId = await readDb.OpportunityApplications
            .Where(a => a.Id == applicationId)
            .Select(a => (Guid?)a.Artist.UserId)
            .FirstOrDefaultAsync();
        return userId.HasValue
            ? await context.Users.OfType<ArtistManagerEntity>().FirstOrDefaultAsync(u => u.Id == userId)
            : null;
    }
}
