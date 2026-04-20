using Concertable.Application.Interfaces.Auth;
using Concertable.Core.Entities;
using Concertable.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Services.Auth;

public class VenueManagerLoader : IUserLoader
{
    private readonly IdentityDbContext context;

    public VenueManagerLoader(IdentityDbContext context)
    {
        this.context = context;
    }

    public Task<UserEntity> LoadAsync(UserEntity user) =>
        context.Users.OfType<VenueManagerEntity>().Where(u => u.Id == user.Id).Cast<UserEntity>().FirstAsync();
}
