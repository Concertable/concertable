using Concertable.Core.Entities.Interfaces;

namespace Concertable.Core.Entities;

public class SocialMediaEntity : IEntity
{
    public int Id { get; set; }
    public int Site { get; set; }
    public required string Handle { get; set; }
    public int ArtistId { get; set; }
    public ArtistEntity Artist { get; set; } = null!;
}
