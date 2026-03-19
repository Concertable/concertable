using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class VenueManagerRepository : Repository<VenueManagerEntity>, IVenueManagerRepository
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
}
