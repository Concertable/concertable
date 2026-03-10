using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Exceptions;

namespace Infrastructure.Services;

public class CurrentUser : ICurrentUser
{
    public static readonly CurrentUser Unauthenticated = new();

    private readonly UserDto? _dto;
    private readonly User? _entity;

    private CurrentUser() { }

    public CurrentUser(UserDto dto, User? entity = null)
    {
        _dto = dto;
        _entity = entity;
    }

    public int? Id => _dto?.Id;

    public int GetId() => Id ?? throw new UnauthorizedException("User not authenticated");

    public UserDto Get() =>
        _dto ?? throw new UnauthorizedException("User not authenticated");

    public UserDto? GetOrDefault() => _dto;

    public User GetEntity() =>
        _entity ?? throw new UnauthorizedException("User not authenticated");

    public Role GetRole() =>
        Get().Role ?? throw new BadRequestException("User has no roles assigned.");
}
