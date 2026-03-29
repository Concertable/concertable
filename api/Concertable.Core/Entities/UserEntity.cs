using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Enums;
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
    public string? County { get; set; }
    public string? Town { get; set; }
    public Point? Location { get; set; }
    public string? StripeId { get; set; }

    public ICollection<MessageEntity> SentMessages { get; set; } = [];
    public ICollection<MessageEntity> ReceivedMessages { get; set; } = [];
    public PreferenceEntity? Preference { get; set; }
    public ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = [];
}

public class VenueManagerEntity : UserEntity
{
    public VenueEntity? Venue { get; set; }
}

public class ArtistManagerEntity : UserEntity
{
    public ArtistEntity? Artist { get; set; }
}

public class CustomerEntity : UserEntity
{
    public ICollection<TicketEntity> Tickets { get; set; } = [];
}
