using Microsoft.Extensions.DependencyInjection;

namespace Concertable.User.Infrastructure.Services.Auth;

internal class UserRegister : IUserRegister
{
    private readonly IServiceProvider serviceProvider;

    public UserRegister(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public Task RegisterAsync(string email, string passwordHash, Role role) =>
        serviceProvider.GetRequiredKeyedService<IUserRegister>(role).RegisterAsync(email, passwordHash, role);
}
