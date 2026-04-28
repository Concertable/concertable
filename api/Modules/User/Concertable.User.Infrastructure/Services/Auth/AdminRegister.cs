using Concertable.User.Infrastructure.Data;

namespace Concertable.User.Infrastructure.Services.Auth;

internal class AdminRegister : IUserRegister
{
    private readonly UserDbContext context;

    public AdminRegister(UserDbContext context)
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
