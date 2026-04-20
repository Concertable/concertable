using NetTopologySuite.Geometries;

namespace Concertable.Identity.Domain;

public class UserEntity : IGuidEntity
{
    protected UserEntity() { }

    protected UserEntity(string email, string passwordHash, Role role)
    {
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; private set; }
    public Address? Address { get; set; }
    public Point? Location { get; set; }
    public string StripeCustomerId { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public bool IsEmailVerified { get; private set; }

    public ICollection<RefreshTokenEntity> RefreshTokens { get; private set; } = [];
    public ICollection<EmailVerificationTokenEntity> EmailVerificationTokens { get; private set; } = [];
    public ICollection<PasswordResetTokenEntity> PasswordResetTokens { get; private set; } = [];

    public void VerifyEmail() => IsEmailVerified = true;
}

public abstract class ManagerEntity : UserEntity
{
    protected ManagerEntity() { }

    protected ManagerEntity(string email, string passwordHash, Role role)
        : base(email, passwordHash, role) { }

    public string StripeAccountId { get; set; } = string.Empty;
}

public class VenueManagerEntity : ManagerEntity
{
    private VenueManagerEntity() { }

    private VenueManagerEntity(string email, string passwordHash)
        : base(email, passwordHash, Role.VenueManager) { }

    public static VenueManagerEntity Create(string email, string passwordHash) => new(email, passwordHash);
}

public class ArtistManagerEntity : ManagerEntity
{
    private ArtistManagerEntity() { }

    private ArtistManagerEntity(string email, string passwordHash)
        : base(email, passwordHash, Role.ArtistManager) { }

    public static ArtistManagerEntity Create(string email, string passwordHash) => new(email, passwordHash);
}

public class CustomerEntity : UserEntity
{
    private CustomerEntity() { }

    private CustomerEntity(string email, string passwordHash)
        : base(email, passwordHash, Role.Customer) { }

    public static CustomerEntity Create(string email, string passwordHash) => new(email, passwordHash);
}

public class AdminEntity : UserEntity
{
    private AdminEntity() { }

    private AdminEntity(string email, string passwordHash)
        : base(email, passwordHash, Role.Admin) { }

    public static AdminEntity Create(string email, string passwordHash) => new(email, passwordHash);
}
