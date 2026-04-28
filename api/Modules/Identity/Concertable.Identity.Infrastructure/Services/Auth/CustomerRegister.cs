using Concertable.Identity.Infrastructure.Data;

namespace Concertable.Identity.Infrastructure.Services.Auth;

internal class CustomerRegister : IUserRegister
{
    private readonly IdentityDbContext context;

    public CustomerRegister(IdentityDbContext context)
    {
        this.context = context;
    }

    public async Task RegisterAsync(string email, string passwordHash, Role role)
    {
        var user = CustomerEntity.Create(email, passwordHash);
        context.Users.Add(user);
        await context.SaveChangesAsync();
    }
}
