

namespace Core.Entities;

public class SocialMediaEntity : BaseEntity
{
    public int Site { get; set; }
    public required string Handle { get; set; }
    public int ArtistId { get; set; }
    public ArtistEntity Artist { get; set; } = null!;
}
