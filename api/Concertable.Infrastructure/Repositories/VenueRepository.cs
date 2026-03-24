using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class VenueRepository : Repository<VenueEntity>, IVenueRepository
{
    public VenueRepository(ApplicationDbContext context) : base(context)
    {
    }

    public new async Task<VenueEntity?> GetByIdAsync(int id)
    {
        return await context.Venues
            .Where(v => v.Id == id)
            .Include(v => v.User)
            .FirstOrDefaultAsync();
    }

    public async Task<VenueEntity?> GetByUserIdAsync(Guid id)
    {
        return await context.Venues
            .Where(v => v.UserId == id)
            .Include(v => v.User)
            .FirstOrDefaultAsync();
    }

    public async Task<int?> GetIdByUserIdAsync(Guid userId)
    {
        return await context.Venues
            .Where(a => a.UserId == userId)
            .Select(a => a.Id)
            .FirstOrDefaultAsync();
    }

}
