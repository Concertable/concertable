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

    }
}
