using Concertable.Identity.Infrastructure.Data;

namespace Concertable.Identity.Infrastructure.Services.Auth;

internal class ArtistManagerRegister : IUserRegister
{
    private readonly IdentityDbContext context;

    public ArtistManagerRegister(IdentityDbContext context)
    {
        this.context = context;
    }

    public async Task RegisterAsync(RegisterRequest request, string passwordHash)
    {
        var manager = ArtistManagerEntity.Create(request.Email, passwordHash);
        context.Users.Add(manager);
        await context.SaveChangesAsync();
    }
}
