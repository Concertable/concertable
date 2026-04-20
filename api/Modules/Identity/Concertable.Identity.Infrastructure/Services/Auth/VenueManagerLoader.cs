using Concertable.Core.Entities;
using Concertable.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Identity.Infrastructure.Services.Auth;

internal class VenueManagerLoader : IUserLoader
{
    private readonly IdentityDbContext context;

    public VenueManagerLoader(IdentityDbContext context)
    {
        this.context = context;
    }

    public Task<UserEntity> LoadAsync(UserEntity user) =>
        context.Users.OfType<VenueManagerEntity>().Where(u => u.Id == user.Id).Cast<UserEntity>().FirstAsync();
}
