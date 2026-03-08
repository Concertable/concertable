using Application.DTOs;
using Application.Interfaces;
using Core.Entities.Identity;
using Core.Exceptions;

namespace Infrastructure.Services;

public class CurrentUser : ICurrentUser
{
    private UserDto? _dto;
    private ApplicationUser? _entity;

    public int? Id => _dto?.Id;

    public int GetId() => Id ?? throw new UnauthorizedException("User not authenticated");

    public UserDto Get() =>
        _dto ?? throw new UnauthorizedException("User not authenticated");

    public UserDto? GetOrDefault() => _dto;

    public ApplicationUser GetEntity() =>
        _entity ?? throw new UnauthorizedException("User not authenticated");

    public string GetFirstRole() =>
        Get().Role ?? throw new BadRequestException("User has no roles assigned.");

    /// <summary>
    /// Called by middleware to populate the current user. Do not call from application code.
    /// </summary>
    public void Set(UserDto dto, ApplicationUser entity)
    {
        _dto = dto;
        _entity = entity;
    }
}
