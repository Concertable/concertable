using Concertable.Core.Entities.Interfaces;

namespace Concertable.Core.Entities;

public class VideoEntity : IEntity
{
    public int Id { get; set; }
    public int ArtistId { get; set; }
    public required string Url { get; set; }
    public ArtistEntity Artist { get; set; } = null!;
}
