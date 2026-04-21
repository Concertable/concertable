using Concertable.Data.Application;
using Concertable.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Identity.Infrastructure.Repositories;

internal class UserRepository : IUserRepository
{
    private readonly IdentityDbContext context;
    private readonly IReadDbContext readDb;

    public UserRepository(IdentityDbContext context, IReadDbContext readDb)
    {
        this.context = context;
        this.readDb = readDb;
    }

    public async Task<UserEntity> AddAsync(UserEntity entity)
    {
        await context.Users.AddAsync(entity);
        return entity;
    }

    public async Task<IEnumerable<UserEntity>> AddRangeAsync(IEnumerable<UserEntity> entities)
    {
        await context.Users.AddRangeAsync(entities);
        return entities;
    }

    public async Task<IEnumerable<UserEntity>> GetAllAsync() =>
        await context.Users.ToListAsync();

    public void Remove(UserEntity entity) => context.Users.Remove(entity);

    public void Update(UserEntity entity) => context.Users.Update(entity);

    public async Task SaveChangesAsync() => await context.SaveChangesAsync();

    public async Task<UserEntity?> GetByIdAsync(Guid id) =>
        await context.Users.FindAsync(id);

    public bool Exists(Guid id) => context.Users.Any(e => e.Id == id);

    public Task<bool> ExistsByEmailAsync(string email) =>
        context.Users.AnyAsync(u => u.Email == email);

    public Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<UserEntity?> GetByApplicationIdAsync(int applicationId)
    {
        var userId = await readDb.OpportunityApplications
            .Where(a => a.Id == applicationId)
            .Select(a => (Guid?)a.Artist.UserId)
            .FirstOrDefaultAsync();
        return userId.HasValue ? await context.Users.FirstOrDefaultAsync(u => u.Id == userId) : null;
    }

    public async Task<UserEntity?> GetByConcertIdAsync(int concertId)
    {
        var userId = await readDb.Concerts
            .Where(e => e.Id == concertId)
            .Select(e => (Guid?)e.Booking.Application.Artist.UserId)
            .FirstOrDefaultAsync();
        return userId.HasValue ? await context.Users.FirstOrDefaultAsync(u => u.Id == userId) : null;
    }

    public Task<Guid?> GetIdByApplicationIdAsync(int applicationId) =>
        readDb.OpportunityApplications
            .Where(a => a.Id == applicationId)
            .Select(a => (Guid?)a.Artist.UserId)
            .FirstOrDefaultAsync();

    public Task<Guid?> GetIdByConcertIdAsync(int concertId) =>
        readDb.Concerts
            .Where(e => e.Id == concertId)
            .Select(e => (Guid?)e.Booking.Application.Artist.UserId)
            .FirstOrDefaultAsync();
}
