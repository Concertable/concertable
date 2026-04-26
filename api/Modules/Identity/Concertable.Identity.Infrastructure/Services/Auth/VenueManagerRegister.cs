using Concertable.Identity.Infrastructure.Data;

namespace Concertable.Identity.Infrastructure.Services.Auth;

internal class VenueManagerRegister : IUserRegister
{
    private readonly IdentityDbContext context;

    public VenueManagerRegister(IdentityDbContext context)
    {
        this.context = context;
    }

    public async Task RegisterAsync(RegisterRequest request, string passwordHash)
    {
        var manager = VenueManagerEntity.Create(request.Email, passwordHash);
        context.Users.Add(manager);
        await context.SaveChangesAsync();
    }
}
