using Microsoft.Extensions.DependencyInjection;

namespace Concertable.User.Infrastructure.Services.Auth;

internal class UserLoader : IUserLoader
{
    private readonly IServiceProvider serviceProvider;

    public UserLoader(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public Task<UserEntity> LoadAsync(UserEntity user) =>
        serviceProvider.GetRequiredKeyedService<IUserLoader>(user.Role).LoadAsync(user);
}
