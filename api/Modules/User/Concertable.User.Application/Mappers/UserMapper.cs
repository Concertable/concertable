using System.Collections.Frozen;

namespace Concertable.User.Application.Mappers;

internal class UserMapper : IUserMapper
{
    private static readonly FrozenDictionary<Role, IUserMapper> mappers =
        new Dictionary<Role, IUserMapper>
        {
            [Role.VenueManager] = new VenueManagerMapper(),
            [Role.ArtistManager] = new ArtistManagerMapper(),
            [Role.Customer] = new CustomerMapper(),
            [Role.Admin] = new AdminMapper(),
        }.ToFrozenDictionary();

    public IUser ToDto(UserEntity entity) => mappers[entity.Role].ToDto(entity);
}
