using Application.DTOs;
using Core.Entities;

namespace Application.Mappers;

public static class UserMappers
{
    public static UserDto ToDto(this UserEntity user) => new()
    {
        Id = user.Id,
        Email = user.Email ?? string.Empty,
        Latitude = user.Location.ToLatitude(),
        Longitude = user.Location.ToLongitude(),
        County = user.County,
        Town = user.Town,
        Role = user.Role
    };
}
