using Concertable.Application.Interfaces;
using Concertable.Application.Exceptions;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace Concertable.Infrastructure.Services;

public class CurrentUserAccessor : ICurrentUser
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    private CurrentUser? Current =>
        httpContextAccessor.HttpContext?.Items[nameof(CurrentUser)] as CurrentUser;

    public Guid? Id => Current?.Id;
    public Guid GetId() => Current?.Id ?? throw new UnauthorizedException("User not authenticated");
    public IUser Get() => Current?.Get() ?? throw new UnauthorizedException("User not authenticated");
    public IUser? GetOrDefault() => Current?.GetOrDefault();
    public UserEntity GetEntity() => Current?.GetEntity() ?? throw new UnauthorizedException("User not authenticated");
    public T GetEntity<T>() where T : UserEntity => Current?.GetEntity<T>() ?? throw new UnauthorizedException("User not authenticated");
    public Role GetRole() => Current?.GetRole() ?? throw new UnauthorizedException("User not authenticated");
}
