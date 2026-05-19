using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.Seeding.Factories;

public static class UserFactory
{
    public static UserEntity Customer(string email, string passwordHash)
        => New<UserEntity>()
            .With(nameof(UserEntity.Email), email)
            .With(nameof(UserEntity.PasswordHash), passwordHash)
            .With(nameof(UserEntity.Role), Role.Customer)
            .With(nameof(UserEntity.IsEmailVerified), true);

    public static UserEntity ArtistManager(string email, string passwordHash)
        => New<UserEntity>()
            .With(nameof(UserEntity.Email), email)
            .With(nameof(UserEntity.PasswordHash), passwordHash)
            .With(nameof(UserEntity.Role), Role.ArtistManager)
            .With(nameof(UserEntity.IsEmailVerified), true);

    public static UserEntity VenueManager(string email, string passwordHash)
        => New<UserEntity>()
            .With(nameof(UserEntity.Email), email)
            .With(nameof(UserEntity.PasswordHash), passwordHash)
            .With(nameof(UserEntity.Role), Role.VenueManager)
            .With(nameof(UserEntity.IsEmailVerified), true);

    public static UserEntity Admin(string email, string passwordHash)
        => New<UserEntity>()
            .With(nameof(UserEntity.Email), email)
            .With(nameof(UserEntity.PasswordHash), passwordHash)
            .With(nameof(UserEntity.Role), Role.Admin)
            .With(nameof(UserEntity.IsEmailVerified), true);
}
