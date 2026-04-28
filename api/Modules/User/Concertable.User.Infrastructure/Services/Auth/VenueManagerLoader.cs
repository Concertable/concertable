using Concertable.User.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.User.Infrastructure.Services.Auth;

internal class VenueManagerLoader : IUserLoader
{
    private readonly UserDbContext context;

    public VenueManagerLoader(UserDbContext context)
    {
        this.context = context;
    }

    public Task<UserEntity> LoadAsync(UserEntity user) =>
        context.Users.OfType<VenueManagerEntity>().Where(u => u.Id == user.Id).Cast<UserEntity>().FirstAsync();
}
