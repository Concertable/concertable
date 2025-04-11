

using Core.Entities;

namespace Core.Entities
{
    public class Genre : BaseEntity
    {
        public string Name { get; set; }
        public ICollection<EventGenre> EventGenres { get; } = new List<EventGenre>();
        public ICollection<ListingGenre> ListingGenres { get; set; } = new List<ListingGenre>();
        public ICollection<ArtistGenre> ArtistGenres { get; set; } = new List<ArtistGenre>();
    }
}
