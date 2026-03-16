using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : BaseRepository<UserEntity>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<UserEntity> GetByApplicationIdAsync(int applicationId)
    {
        var query = context.ConcertApplications
            .Where(a => a.Id == applicationId)
            .Select(a => a.Artist.User);

        return await query.FirstAsync();
    }

    public async Task<UserEntity> GetByConcertIdAsync(int concertId)
    {
        var query = context.Concerts
             .Where(e => e.Id == concertId)
             .Select(e => e.Application.Artist.User);

        return await query.FirstAsync();
    }

    public async Task<int> GetIdByApplicationIdAsync(int applicationId)
    {
        var query = context.ConcertApplications
            .Where(a => a.Id == applicationId)
            .Select(a => a.Artist.UserId);

        return await query.FirstAsync();
    }

    public async Task<int> GetIdByConcertIdAsync(int concertId)
    {
        var query = context.Concerts
            .Where(e => e.Id == concertId)
            .Select(e => e.Application.Artist.UserId);

        return await query.FirstAsync();
    }

    public Task<bool> ExistsByEmailAsync(string email)
    {
        return context.Users.AnyAsync(u => u.Email == email);
    }
}
