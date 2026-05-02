namespace Concertable.User.Application.Mappers;

internal class ArtistManagerMapper : IUserMapper
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
            County = am.Address?.County,
            Town = am.Address?.Town,
            ArtistId = am.ArtistId,
            IsEmailVerified = am.IsEmailVerified
        };
    }
}
