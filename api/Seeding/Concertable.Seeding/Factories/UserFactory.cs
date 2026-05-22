namespace Concertable.Seeding.Factories;

public static class UserFactory
{
    public static UserEntity Customer(string email, string passwordHash)
    {
        var u = UserEntity.Create(email, passwordHash, Role.Customer);
        u.VerifyEmail();
        return u;
    }

    public static UserEntity ArtistManager(string email, string passwordHash)
    {
        var u = UserEntity.Create(email, passwordHash, Role.ArtistManager);
        u.VerifyEmail();
        return u;
    }

    public static UserEntity VenueManager(string email, string passwordHash)
    {
        var u = UserEntity.Create(email, passwordHash, Role.VenueManager);
        u.VerifyEmail();
        return u;
    }

    public static UserEntity Admin(string email, string passwordHash)
    {
        var u = UserEntity.Create(email, passwordHash, Role.Admin);
        u.VerifyEmail();
        return u;
    }
}
