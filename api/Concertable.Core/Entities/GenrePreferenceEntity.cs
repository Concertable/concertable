using Concertable.Core.Interfaces;

namespace Concertable.Core.Entities;

public class GenrePreferenceEntity : IIdEntity, IGenreJoin
{
    public int Id { get; private set; }
    public int PreferenceId { get; set; }
    public int GenreId { get; set; }
    public GenreEntity Genre { get; set; } = null!;
    public PreferenceEntity Preference { get; set; } = null!;
}
