namespace Concertable.User.Application.Mappers;

internal class VenueManagerMapper : IUserMapper
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
            County = vm.Address?.County,
            Town = vm.Address?.Town,
            VenueId = null,
            IsEmailVerified = vm.IsEmailVerified
        };
    }
}
