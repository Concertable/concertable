using Concertable.Application.Interfaces.Auth;
using Concertable.Core.Entities;
using Concertable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Services.Auth;

public class VenueManagerLoader(ApplicationDbContext context) : IUserLoader
{
    public Task<UserEntity> LoadAsync(Guid id) =>
        context.Users
            .OfType<VenueManagerEntity>()
            .Include(u => u.Venue)
            .Where(u => u.Id == id)
            .Cast<UserEntity>()
            .FirstAsync();
}
