using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public class ArtistManagerMapper : IUserMapper
{
    public IUser ToDto(UserEntity entity)
    {
        var am = (ArtistManagerEntity)entity;
        return new ArtistManagerDto
        {
            Id = am.Id,
            Email = am.Email ?? string.Empty,
            Role = am.Role,
            Latitude = am.Location.ToLatitude(),
            Longitude = am.Location.ToLongitude(),
            County = am.County,
            Town = am.Town,
            ArtistId = am.Artist?.Id,
            IsEmailVerified = am.IsEmailVerified
        };
    }
}
