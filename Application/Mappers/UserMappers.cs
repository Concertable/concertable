using Application.DTOs;
using Common.Helpers;
using Core.Entities.Identity;

namespace Application.Mappers
{
    public static class UserMappers
    {
        public static UserDto ToDto(this ApplicationUser user) => new()
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Latitude = LocationHelper.GetLatitude(user.Location),
            Longitude = LocationHelper.GetLongitude(user.Location),
            County = user.County,
            Town = user.Town
        };
    }
}
