using Concertable.User.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.User.Infrastructure.Services.Auth;

internal class CustomerLoader : IUserLoader
{
    private readonly UserDbContext context;

    public CustomerLoader(UserDbContext context)
    {
        this.context = context;
    }

    public Task<UserEntity> LoadAsync(UserEntity user) =>
        context.Users.OfType<CustomerEntity>().Where(u => u.Id == user.Id).Cast<UserEntity>().FirstAsync();
}
