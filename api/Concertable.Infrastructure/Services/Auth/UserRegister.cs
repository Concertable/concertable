using Concertable.Application.Interfaces.Auth;
using Concertable.Application.Requests;
using Concertable.Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Services.Auth;

public class UserRegister : IUserRegister
{
    private readonly IServiceProvider serviceProvider;

    public UserRegister(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public Task RegisterAsync(RegisterRequest request, string passwordHash) =>
        serviceProvider.GetRequiredKeyedService<IUserRegister>(request.Role).RegisterAsync(request, passwordHash);
}
