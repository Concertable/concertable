using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
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

        public async Task<IEnumerable<VenueHeaderDto>> GetHeadersByAmountAsync(int amount)
        {
            return await context.Venues
                .Include(v => v.User)
                .OrderBy(v => v.Id)
                .Take(amount)
                .Select(v => new VenueHeaderDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    ImageUrl = v.ImageUrl,
                    County = v.User.County,
                    Town = v.User.Town,
                    Latitude = v.User.Location != null ? v.User.Location.Y : (double?)null,
                    Longitude = v.User.Location != null ? v.User.Location.X : (double?)null
                })
                .ToListAsync();
        }
    }
}
