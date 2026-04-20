using Concertable.Application.Interfaces;

namespace Concertable.Application.Mappers;

public class UserMapper : IUserMapper
{
    private readonly IDictionary<Role, IUserMapper> mappers = new Dictionary<Role, IUserMapper>
    {
        { Role.VenueManager, new VenueManagerMapper() },
        { Role.ArtistManager, new ArtistManagerMapper() },
        { Role.Customer, new CustomerMapper() },
        { Role.Admin, new AdminMapper() }
    };

    public IUser ToDto(UserEntity entity) => mappers[entity.Role].ToDto(entity);
}
