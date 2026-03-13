using Core.Enums;
using NetTopologySuite.Geometries;

namespace Core.Entities;

/// <summary>
/// User for auth. Role is on the entity (Customer, VenueManager, ArtistManager, Admin).
/// </summary>
public class User : BaseEntity
{
    public required string Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public required Role Role { get; set; }
    public string? County { get; set; }
    public string? Town { get; set; }
    public Point? Location { get; set; }
    public string? StripeId { get; set; }

    public ICollection<Message> SentMessages { get; set; } = [];
    public ICollection<Message> ReceivedMessages { get; set; } = [];
    public Preference? Preference { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}

public class VenueManager : User
{
    public Venue? Venue { get; set; }
}

public class ArtistManager : User
{
    public Artist? Artist { get; set; }
}

public class Customer : User
{
    public ICollection<Ticket> Tickets { get; set; } = [];
}
