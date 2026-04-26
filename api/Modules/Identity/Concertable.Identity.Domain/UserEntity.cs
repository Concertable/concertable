using Concertable.Identity.Domain.Events;
using NetTopologySuite.Geometries;

namespace Concertable.Identity.Domain;

public class UserEntity : IGuidEntity, IEventRaiser
{
    private readonly EventRaiser _events = new();

    protected UserEntity() { }

    protected UserEntity(string email, string passwordHash, Role role)
    {
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        _events.Raise(new UserCreatedDomainEvent(this));
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; private set; }
    public Address? Address { get; private set; }
    public Point? Location { get; private set; }
    public string Avatar { get; set; } = string.Empty;
    public bool IsEmailVerified { get; private set; }

    public IReadOnlyList<IDomainEvent> DomainEvents => _events.DomainEvents;
    public void ClearDomainEvents() => _events.Clear();

    public ICollection<RefreshTokenEntity> RefreshTokens { get; private set; } = [];
    public ICollection<EmailVerificationTokenEntity> EmailVerificationTokens { get; private set; } = [];
    public ICollection<PasswordResetTokenEntity> PasswordResetTokens { get; private set; } = [];

    public void VerifyEmail() => IsEmailVerified = true;

    public void UpdateLocation(Point location, Address? address = null)
    {
        Location = location;
        Address = address;
        _events.Raise(new UserLocationUpdatedDomainEvent(Id, location, address));
    }
}

public abstract class ManagerEntity : UserEntity
{
    protected ManagerEntity() { }

    protected ManagerEntity(string email, string passwordHash, Role role)
        : base(email, passwordHash, role) { }

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
