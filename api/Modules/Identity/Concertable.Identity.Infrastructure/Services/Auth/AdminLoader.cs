using Concertable.Core.Entities;
using Concertable.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Identity.Infrastructure.Services.Auth;

public class AdminLoader : IUserLoader
{
    private readonly IdentityDbContext context;

    public AdminLoader(IdentityDbContext context)
    {
        this.context = context;
    }

    public Task<UserEntity> LoadAsync(UserEntity user) =>
        context.Users.OfType<AdminEntity>().Where(u => u.Id == user.Id).Cast<UserEntity>().FirstAsync();
}
