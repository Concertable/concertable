using Concertable.Identity.Infrastructure.Data;

namespace Concertable.Identity.Infrastructure.Services.Auth;

internal class CustomerRegister : IUserRegister
{
    private readonly IdentityDbContext context;

    public CustomerRegister(IdentityDbContext context)
    {
        this.context = context;
    }

    public async Task RegisterAsync(RegisterRequest request, string passwordHash)
    {
        var user = CustomerEntity.Create(request.Email, passwordHash);
        context.Users.Add(user);
        await context.SaveChangesAsync();
    }
}
