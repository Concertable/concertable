using Application.Interfaces;
using Concertable.Infrastructure.Data;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ArtistManagerRepository : GuidRepository<ArtistManagerEntity>, IArtistManagerRepository
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

    public async Task<ArtistManagerEntity?> GetByApplicationIdAsync(int applicationId)
    {
        return await context.Users
            .OfType<ArtistManagerEntity>()
            .Where(u => u.Id == context.ConcertApplications
                .Where(a => a.Id == applicationId)
                .Select(a => a.Artist.UserId)
                .First())
            .FirstOrDefaultAsync();
    }
}
