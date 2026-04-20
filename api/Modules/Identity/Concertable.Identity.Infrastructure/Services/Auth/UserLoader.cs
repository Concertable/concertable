using Concertable.Application.Interfaces.Auth;
using Concertable.Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Services.Auth;

public class UserLoader : IUserLoader
{
    private readonly IServiceProvider serviceProvider;

    public UserLoader(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public Task<UserEntity> LoadAsync(UserEntity user) =>
        serviceProvider.GetRequiredKeyedService<IUserLoader>(user.Role).LoadAsync(user);
}
