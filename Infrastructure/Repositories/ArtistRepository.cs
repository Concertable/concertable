using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ArtistRepository : Repository<Artist>, IArtistRepository
    {
        public ArtistRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Artist?> GetByUserIdAsync(int id)
        {
            return await context.Artists
                .Where(v => v.UserId == id)
                .Include(a => a.ArtistGenres)
                    .ThenInclude(ag => ag.Genre)
                .Include(a => a.User)
                .FirstOrDefaultAsync();
        }

        public new async Task<Artist?> GetByIdAsync(int id)
        {
            return await context.Artists
                .Where(v => v.Id == id)
                .Include(a => a.ArtistGenres)
                    .ThenInclude(ag => ag.Genre)
                .Include(a => a.User)
                .FirstOrDefaultAsync();
        }

        public async Task<int?> GetIdByUserIdAsync(int id)
        {
            return await context.Artists
                .Where(a => a.UserId == id)
                .Select(a => a.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ArtistHeaderDto>> GetHeadersByAmountAsync(int amount)
        {
            return await context.Artists
                .Include(a => a.User)
                .OrderBy(a => a.Id)
                .Take(amount)
                .Select(a => new ArtistHeaderDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    ImageUrl = a.ImageUrl,
                    County = a.User.County,
                    Town = a.User.Town,
                    Latitude = a.User.Location != null ? a.User.Location.Y : (double?)null,
                    Longitude = a.User.Location != null ? a.User.Location.X : (double?)null
                })
                .ToListAsync();
        }
    }
}
