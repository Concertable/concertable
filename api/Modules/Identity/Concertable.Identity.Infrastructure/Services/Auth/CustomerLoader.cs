using Concertable.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Identity.Infrastructure.Services.Auth;

internal class CustomerLoader : IUserLoader
{
    private readonly IdentityDbContext context;

    public CustomerLoader(IdentityDbContext context)
    {
        this.context = context;
    }

    public Task<UserEntity> LoadAsync(UserEntity user) =>
        context.Users.OfType<CustomerEntity>().Where(u => u.Id == user.Id).Cast<UserEntity>().FirstAsync();
}
