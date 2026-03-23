using Core.Interfaces;

namespace Core.Entities;

public class GenrePreferenceEntity : BaseEntity, IGenreJoin
{
    public int PreferenceId { get; set; }
    public int GenreId { get; set; }
    public GenreEntity Genre { get; set; } = null!;
    public PreferenceEntity Preference { get; set; } = null!;
}
