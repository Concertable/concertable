using Application.DTOs;
using Common.Helpers;
using Core.Entities;

namespace Application.Mappers;

public static class UserMappers
{
    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Latitude = LocationHelper.GetLatitude(user.Location),
        Longitude = LocationHelper.GetLongitude(user.Location),
        County = user.County,
        Town = user.Town,
        Role = user.Role
    };
}
