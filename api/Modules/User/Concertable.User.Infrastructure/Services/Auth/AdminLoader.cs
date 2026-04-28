using Concertable.User.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.User.Infrastructure.Services.Auth;

internal class AdminLoader : IUserLoader
{
    private readonly UserDbContext context;

    public AdminLoader(UserDbContext context)
    {
        this.context = context;
    }

    public Task<UserEntity> LoadAsync(UserEntity user) =>
        context.Users.OfType<AdminEntity>().Where(u => u.Id == user.Id).Cast<UserEntity>().FirstAsync();
}
