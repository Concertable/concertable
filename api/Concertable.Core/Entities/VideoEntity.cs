using Core.Entities.Interfaces;

namespace Core.Entities;

public class VideoEntity : IEntity
{
    public int Id { get; set; }
    public int ArtistId { get; set; }
    public required string Url { get; set; }
    public ArtistEntity Artist { get; set; } = null!;
}
