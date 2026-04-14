using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Enums;
using Concertable.Core.ValueObjects;
using NetTopologySuite.Geometries;

namespace Concertable.Core.Entities;

/// <summary>
/// UserEntity for auth. Role is on the entity (Customer, VenueManager, ArtistManager, Admin).
/// </summary>
public class UserEntity : IGuidEntity
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public required Role Role { get; set; }
    public Address? Address { get; set; }
    public Point? Location { get; set; }
    public string StripeCustomerId { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; }

    public ICollection<MessageEntity> SentMessages { get; set; } = [];
    public ICollection<MessageEntity> ReceivedMessages { get; set; } = [];
    public ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = [];
    public ICollection<EmailVerificationTokenEntity> EmailVerificationTokens { get; set; } = [];
    public ICollection<PasswordResetTokenEntity> PasswordResetTokens { get; set; } = [];
}


public abstract class ManagerEntity : UserEntity
{
    public string StripeAccountId { get; set; } = string.Empty;
}

public class VenueManagerEntity : ManagerEntity
{
    public VenueEntity? Venue { get; set; }
}

public class ArtistManagerEntity : ManagerEntity
{
    public ArtistEntity? Artist { get; set; }
}

public class CustomerEntity : UserEntity
{
    public PreferenceEntity? Preference { get; set; }
    public ICollection<TicketEntity> Tickets { get; set; } = [];
}

public class AdminEntity : UserEntity { }
