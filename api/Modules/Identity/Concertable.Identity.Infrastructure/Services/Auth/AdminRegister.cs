using Concertable.Identity.Infrastructure.Data;

namespace Concertable.Identity.Infrastructure.Services.Auth;

internal class AdminRegister : IUserRegister
{
    private readonly IdentityDbContext context;

    public AdminRegister(IdentityDbContext context)
    {
        this.context = context;
    }

    public async Task RegisterAsync(string email, string passwordHash, Role role)
    {
        var admin = AdminEntity.Create(email, passwordHash);
        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}
