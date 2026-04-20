using Concertable.Core.Entities;
using Concertable.Identity.Infrastructure.Data;

namespace Concertable.Identity.Infrastructure.Services.Auth;

public class AdminRegister : IUserRegister
{
    private readonly IdentityDbContext context;

    public AdminRegister(IdentityDbContext context)
    {
        this.context = context;
    }

    public async Task RegisterAsync(RegisterRequest request, string passwordHash)
    {
        var admin = AdminEntity.Create(request.Email, passwordHash);
        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}
