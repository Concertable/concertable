using Core.Entities;
using Application.Interfaces;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class VenueRepository : Repository<Venue>, IVenueRepository
    {
        public VenueRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Venue> GetByIdAsync(int id)
        {
            return await context.Venues
                .Where(v => v.Id == id)
                .Include(v => v.User)
                .FirstAsync();
        }

        public async Task<Venue?> GetByUserIdAsync(int id)
        {
            return await context.Venues
                .Where(v => v.UserId == id)
                .Include(v => v.User)
                .FirstOrDefaultAsync();
        }

        public async Task<int?> GetIdByUserIdAsync(int userId)
        {
            return await context.Venues
                .Where(a => a.UserId == userId)
                .Select(a => a.Id)
                .FirstOrDefaultAsync();
        }
    }
}
