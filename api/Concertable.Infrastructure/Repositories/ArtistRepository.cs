using Concertable.Application.Interfaces;
using Concertable.Infrastructure.Data;
using Concertable.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories;

public class ArtistRepository : Repository<ArtistEntity>, IArtistRepository
{
    public ArtistRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ArtistEntity?> GetByUserIdAsync(Guid id)
    {
        return await context.Artists
            .Where(v => v.UserId == id)
            .Include(a => a.ArtistGenres)
                .ThenInclude(ag => ag.Genre)
            .Include(a => a.User)
            .FirstOrDefaultAsync();
    }

    public async Task<ArtistEntity?> GetDetailsByIdAsync(int id)
    {
        return await context.Artists
            .Where(v => v.Id == id)
            .Include(a => a.ArtistGenres)
                .ThenInclude(ag => ag.Genre)
            .Include(a => a.User)
            .FirstOrDefaultAsync();
    }

    public async Task<int?> GetIdByUserIdAsync(Guid id)
    {
        return await context.Artists
            .Where(a => a.UserId == id)
            .Select(a => a.Id)
            .FirstOrDefaultAsync();
    }

}
