using Concertable.User.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.User.Infrastructure.Services.Auth;

internal class ArtistManagerLoader : IUserLoader
{
    private readonly UserDbContext context;

    public ArtistManagerLoader(UserDbContext context)
    {
        this.context = context;
    }

    public Task<UserEntity> LoadAsync(UserEntity user) =>
        context.Users.OfType<ArtistManagerEntity>().Where(u => u.Id == user.Id).Cast<UserEntity>().FirstAsync();
}
