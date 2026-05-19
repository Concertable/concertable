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
        var user = UserEntity.Create(email, passwordHash, Role.Admin);
        context.Users.Add(user);
        context.AdminProfiles.Add(new AdminProfileEntity(user.Id));
        await context.SaveChangesAsync();
    }
}
