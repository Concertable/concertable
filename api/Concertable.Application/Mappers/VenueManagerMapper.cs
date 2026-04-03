using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public class VenueManagerMapper : IUserMapper
{
    public IUser ToDto(UserEntity entity)
    {
        var vm = (VenueManagerEntity)entity;
        return new VenueManagerDto
        {
            Id = vm.Id,
            Email = vm.Email ?? string.Empty,
            Role = vm.Role,
            Latitude = vm.Location.ToLatitude(),
            Longitude = vm.Location.ToLongitude(),
            County = vm.County,
            Town = vm.Town,
            VenueId = vm.Venue?.Id
        };
    }
}
