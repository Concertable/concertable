using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ArtistManagerRepository : Repository<ArtistManagerEntity>, IArtistManagerRepository
{
    public ArtistManagerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ArtistManagerEntity?> GetByConcertIdAsync(int concertId)
    {
        return await context.Users
            .OfType<ArtistManagerEntity>()
            .Where(u => u.Id == context.Concerts
                .Where(c => c.Id == concertId)
                .Select(c => c.Application.Artist.UserId)
                .First())
            .FirstOrDefaultAsync();
    }
}
