using Concertable.Core.Enums;
using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.Seeding.Factories;

public static class UserFactory
{
    public static CustomerEntity Customer(string email, string passwordHash)
        => New<CustomerEntity>()
            .With(nameof(UserEntity.Email), email)
            .With(nameof(UserEntity.PasswordHash), passwordHash)
            .With(nameof(UserEntity.Role), Role.Customer)
            .With(nameof(UserEntity.IsEmailVerified), true);

    public static ArtistManagerEntity ArtistManager(string email, string passwordHash)
        => New<ArtistManagerEntity>()
            .With(nameof(UserEntity.Email), email)
            .With(nameof(UserEntity.PasswordHash), passwordHash)
            .With(nameof(UserEntity.Role), Role.ArtistManager)
            .With(nameof(UserEntity.IsEmailVerified), true);

    public static VenueManagerEntity VenueManager(string email, string passwordHash)
        => New<VenueManagerEntity>()
            .With(nameof(UserEntity.Email), email)
            .With(nameof(UserEntity.PasswordHash), passwordHash)
            .With(nameof(UserEntity.Role), Role.VenueManager)
            .With(nameof(UserEntity.IsEmailVerified), true);

    public static AdminEntity Admin(string email, string passwordHash)
        => New<AdminEntity>()
            .With(nameof(UserEntity.Email), email)
            .With(nameof(UserEntity.PasswordHash), passwordHash)
            .With(nameof(UserEntity.Role), Role.Admin)
            .With(nameof(UserEntity.IsEmailVerified), true);
}
