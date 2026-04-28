using Concertable.Identity.Infrastructure.Data;

namespace Concertable.Identity.Infrastructure.Services.Auth;

internal class ArtistManagerRegister : IUserRegister
{
    private readonly IdentityDbContext context;

    public ArtistManagerRegister(IdentityDbContext context)
    {
        this.context = context;
    }

    public async Task RegisterAsync(string email, string passwordHash, Role role)
    {
        var manager = ArtistManagerEntity.Create(email, passwordHash);
        context.Users.Add(manager);
        await context.SaveChangesAsync();
    }
}
