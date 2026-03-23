using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

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

    public int? Id => Current.Id;
    public int GetId() => Current.GetId();
    public UserDto Get() => Current.Get();
    public UserDto? GetOrDefault() => Current.GetOrDefault();
    public UserEntity GetEntity() => Current.GetEntity();
    public Role GetRole() => Current.GetRole();
}
