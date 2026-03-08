namespace Core.Entities
{
    public class GenrePreference : BaseEntity
    {
        public int PreferenceId { get; set; }
        public int GenreId { get; set; }
        public Genre Genre { get; set; } = null!;
        public Preference Preference { get; set; } = null!;
    }
}
