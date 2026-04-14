using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public class CustomerMapper : IUserMapper
{
    public IUser ToDto(UserEntity entity) => new CustomerDto
    {
        Id = entity.Id,
        Email = entity.Email ?? string.Empty,
        Role = entity.Role,
        Latitude = entity.Location.ToLatitude(),
        Longitude = entity.Location.ToLongitude(),
        County = entity.Address?.County,
        Town = entity.Address?.Town,
        IsEmailVerified = entity.IsEmailVerified
    };
}
