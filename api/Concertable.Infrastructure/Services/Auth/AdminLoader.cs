using Concertable.Application.Interfaces.Auth;
using Concertable.Core.Entities;
using Concertable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Services.Auth;

public class AdminLoader(ApplicationDbContext context) : IUserLoader
{
    public Task<UserEntity> LoadAsync(Guid id) =>
        context.Users
            .OfType<AdminEntity>()
            .Where(u => u.Id == id)
            .Cast<UserEntity>()
            .FirstAsync();
}
