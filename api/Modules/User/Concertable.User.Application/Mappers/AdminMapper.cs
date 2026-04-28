namespace Concertable.User.Application.Mappers;

internal class AdminMapper : IUserMapper
{
    public IUser ToDto(UserEntity entity) => new AdminDto
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
