

namespace Core.Entities
{
    public class Video : BaseEntity
    {
        public int ArtistId { get; set; }
        public required string Url { get; set; }
        public Artist Artist { get; set; } = null!;
    }
}
