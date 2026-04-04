using Concertable.Application.Interfaces.Auth;
using Concertable.Core.Entities;
using Concertable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Services.Auth;

public class ArtistManagerLoader(ApplicationDbContext context) : IUserLoader
{
    public Task<UserEntity> LoadAsync(Guid id) =>
        context.Users
            .OfType<ArtistManagerEntity>()
            .Include(u => u.Artist)
            .Where(u => u.Id == id)
            .Cast<UserEntity>()
            .FirstAsync();
}
