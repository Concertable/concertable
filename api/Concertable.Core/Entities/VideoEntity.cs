

namespace Core.Entities;

public class VideoEntity : BaseEntity
{
    public int ArtistId { get; set; }
    public required string Url { get; set; }
    public ArtistEntity Artist { get; set; } = null!;
}
