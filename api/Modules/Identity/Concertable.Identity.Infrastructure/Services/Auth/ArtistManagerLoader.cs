using Concertable.Application.Interfaces.Auth;
using Concertable.Core.Entities;
using Concertable.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Services.Auth;

public class ArtistManagerLoader : IUserLoader
{
    private readonly IdentityDbContext context;

    public ArtistManagerLoader(IdentityDbContext context)
    {
        this.context = context;
    }

    public Task<UserEntity> LoadAsync(UserEntity user) =>
        context.Users.OfType<ArtistManagerEntity>().Where(u => u.Id == user.Id).Cast<UserEntity>().FirstAsync();
}
