using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Exceptions;

namespace Infrastructure.Services;

public class CurrentUser : ICurrentUser
{
    public static readonly CurrentUser Unauthenticated = new();

    private readonly UserDto? dto;
    private readonly UserEntity? entity;

    private CurrentUser() { }

    public CurrentUser(UserDto dto, UserEntity? entity = null)
    {
        this.dto = dto;
        this.entity = entity;
    }

    public Guid? Id => dto?.Id;

    public Guid GetId() => Id ?? throw new UnauthorizedException("User not authenticated");

    public UserDto Get() =>
        dto ?? throw new UnauthorizedException("User not authenticated");

    public UserDto? GetOrDefault() => dto;

    public UserEntity GetEntity() =>
        entity ?? throw new UnauthorizedException("User not authenticated");

    public Role GetRole() =>
        Get().Role ?? throw new BadRequestException("User has no roles assigned.");
}
