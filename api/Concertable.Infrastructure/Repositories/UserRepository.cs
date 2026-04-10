using Concertable.Application.Interfaces;
using Concertable.Infrastructure.Data;
using Concertable.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories;

public class UserRepository : GuidRepository<UserEntity>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<UserEntity?> GetByApplicationIdAsync(int applicationId)
    {
        return await context.OpportunityApplications
            .Where(a => a.Id == applicationId)
            .Select(a => a.Artist.User)
            .FirstOrDefaultAsync();
    }

    public async Task<UserEntity?> GetByConcertIdAsync(int concertId)
    {
        return await context.Concerts
            .Where(e => e.Id == concertId)
            .Select(e => e.Application.Artist.User)
            .FirstOrDefaultAsync();
    }

    public async Task<Guid?> GetIdByApplicationIdAsync(int applicationId)
    {
        return await context.OpportunityApplications
            .Where(a => a.Id == applicationId)
            .Select(a => (Guid?)a.Artist.UserId)
            .FirstOrDefaultAsync();
    }

    public async Task<Guid?> GetIdByConcertIdAsync(int concertId)
    {
        return await context.Concerts
            .Where(e => e.Id == concertId)
            .Select(e => (Guid?)e.Application.Artist.UserId)
            .FirstOrDefaultAsync();
    }

    public Task<bool> ExistsByEmailAsync(string email)
    {
        return context.Users.AnyAsync(u => u.Email == email);
    }

    public Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Users
            .Include(u => (u as VenueManagerEntity)!.Venue)
            .Include(u => (u as ArtistManagerEntity)!.Artist)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
}
