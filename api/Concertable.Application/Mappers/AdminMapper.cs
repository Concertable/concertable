using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public class AdminMapper : IUserMapper
{
    public IUser ToDto(UserEntity entity) => new AdminDto
    {
        Id = entity.Id,
        Email = entity.Email ?? string.Empty,
        Role = entity.Role,
        Latitude = entity.Location.ToLatitude(),
        Longitude = entity.Location.ToLongitude(),
        County = entity.County,
        Town = entity.Town,
        IsEmailVerified = entity.IsEmailVerified
    };
}
