using Concertable.Application.Interfaces.Auth;
using Concertable.Core.Entities;
using Concertable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Services.Auth;

public class VenueManagerLoader : IUserLoader
{
    private readonly ApplicationDbContext context;

    public VenueManagerLoader(ApplicationDbContext context)
    {
        this.context = context;
    }

    public Task<UserEntity> LoadAsync(UserEntity user) =>
        context.Users.OfType<VenueManagerEntity>().Where(u => u.Id == user.Id).Cast<UserEntity>().FirstAsync();
}
