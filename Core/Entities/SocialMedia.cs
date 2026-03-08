

namespace Core.Entities
{
    public class SocialMedia : BaseEntity
    {
        public int Site {  get; set; }
        public required string Handle { get; set; }
        public int ArtistId { get; set; }
        public Artist Artist { get; set; } = null!;
    }
}
