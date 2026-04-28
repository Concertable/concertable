using Concertable.User.Infrastructure.Data;

namespace Concertable.User.Infrastructure.Services.Auth;

internal class VenueManagerRegister : IUserRegister
{
    private readonly UserDbContext context;

    public VenueManagerRegister(UserDbContext context)
    {
        this.context = context;
    }

    public async Task RegisterAsync(string email, string passwordHash, Role role)
    {
        var manager = VenueManagerEntity.Create(email, passwordHash);
        context.Users.Add(manager);
        await context.SaveChangesAsync();
    }
}
