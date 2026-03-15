namespace Core.Entities;

public class GenrePreferenceEntity : BaseEntity
{
    public int PreferenceId { get; set; }
    public int GenreId { get; set; }
    public GenreEntity Genre { get; set; } = null!;
    public PreferenceEntity Preference { get; set; } = null!;
}
