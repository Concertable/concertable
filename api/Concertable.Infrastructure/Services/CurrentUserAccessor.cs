using Concertable.Application.Interfaces;
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

    private CurrentUser Current =>
        httpContextAccessor.HttpContext?.Items[nameof(CurrentUser)] as CurrentUser
        ?? CurrentUser.Unauthenticated;

    public Guid? Id => Current.Id;
    public Guid GetId() => Current.GetId();
    public IUser Get() => Current.Get();
    public IUser? GetOrDefault() => Current.GetOrDefault();
    public UserEntity GetEntity() => Current.GetEntity();
    public T GetEntity<T>() where T : UserEntity => Current.GetEntity<T>();
    public Role GetRole() => Current.GetRole();
}
