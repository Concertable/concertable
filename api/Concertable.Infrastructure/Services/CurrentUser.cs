using Concertable.Application.Interfaces;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Core.Exceptions;

namespace Concertable.Infrastructure.Services;

public class CurrentUser : ICurrentUser
{
    public static readonly CurrentUser Unauthenticated = new();

    private readonly IUser? dto;
    private readonly UserEntity? entity;

    private CurrentUser() { }

    public CurrentUser(IUser dto, UserEntity? entity = null)
    {
        this.dto = dto;
        this.entity = entity;
    }

    public Guid? Id => dto?.Id;

    public Guid GetId() => Id ?? throw new UnauthorizedException("User not authenticated");

    public IUser Get() =>
        dto ?? throw new UnauthorizedException("User not authenticated");

    public IUser? GetOrDefault() => dto;

    public UserEntity GetEntity() =>
        entity ?? throw new UnauthorizedException("User not authenticated");

    public T GetEntity<T>() where T : UserEntity =>
        GetEntity() as T ?? throw new UnauthorizedException("Unauthorized");

    public Role GetRole() =>
        Get().Role ?? throw new BadRequestException("User has no roles assigned.");
}
