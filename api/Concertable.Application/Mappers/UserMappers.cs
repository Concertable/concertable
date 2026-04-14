using Concertable.Application.DTOs;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public static class UserMappers
{
    public static UserDto ToDto(this UserEntity user) => new()
    {
        Id = user.Id,
        Email = user.Email ?? string.Empty,
        Latitude = user.Location.ToLatitude(),
        Longitude = user.Location.ToLongitude(),
        County = user.Address?.County,
        Town = user.Address?.Town,
        Role = user.Role
    };
}
