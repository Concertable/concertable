using Concertable.Application.Interfaces.Auth;
using Concertable.Core.Entities;
using Concertable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Services.Auth;

public class ArtistManagerLoader : IUserLoader
{
    private readonly ApplicationDbContext context;

    public ArtistManagerLoader(ApplicationDbContext context)
    {
        this.context = context;
    }

    public Task<UserEntity> LoadAsync(UserEntity user) =>
        context.Users.OfType<ArtistManagerEntity>().Include(u => u.Artist).Where(u => u.Id == user.Id).Cast<UserEntity>().FirstAsync();
}
