

using Core.Entities;

namespace Core.Entities
{
    public class Genre : BaseEntity
    {
        public string Name { get; set; }
        public ICollection<EventGenre> EventGenres { get; } = new List<EventGenre>();
    }
}
