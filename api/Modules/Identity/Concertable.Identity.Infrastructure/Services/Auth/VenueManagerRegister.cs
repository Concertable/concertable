using Concertable.Identity.Infrastructure.Data;

namespace Concertable.Identity.Infrastructure.Services.Auth;

internal class VenueManagerRegister : IUserRegister
{
    private readonly IdentityDbContext context;

    public VenueManagerRegister(IdentityDbContext context)
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
