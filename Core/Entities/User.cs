using NetTopologySuite.Geometries;

namespace Core.Entities;

/// <summary>
/// User for auth. Role is on the entity (Customer, VenueManager, ArtistManager, Admin).
/// </summary>
public class User : BaseEntity
{
    public required string Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    /// <summary>Customer, VenueManager, ArtistManager, Admin</summary>
    public required string Role { get; set; }
    public string? County { get; set; }
    public string? Town { get; set; }
    public Point? Location { get; set; }
    public string? StripeId { get; set; }

    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public Artist? Artist { get; set; }
    public Venue? Venue { get; set; }
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public Preference? Preference { get; set; }
}
