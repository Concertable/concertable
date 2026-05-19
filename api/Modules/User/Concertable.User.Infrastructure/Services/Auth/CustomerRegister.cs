using Concertable.User.Infrastructure.Data;

namespace Concertable.User.Infrastructure.Services.Auth;

internal class CustomerRegister : IUserRegister
{
    private readonly UserDbContext context;

    public CustomerRegister(UserDbContext context)
    {
        this.context = context;
    }

    public async Task RegisterAsync(string email, string passwordHash, Role role)
    {
        var user = UserEntity.Create(email, passwordHash, Role.Customer);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        // CustomerProfile created via UserRegisteredEvent → CustomerProfileCreationHandler
    }
}
