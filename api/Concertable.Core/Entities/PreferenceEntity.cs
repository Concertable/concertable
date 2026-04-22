
namespace Concertable.Core.Entities;

public class PreferenceEntity : IIdEntity, IHasGenreJoins<GenrePreferenceEntity>
{
    private PreferenceEntity() { }

    public int Id { get; private set; }
    public Guid UserId { get; private set; }
    public double RadiusKm { get; private set; }
    public CustomerEntity User { get; set; } = null!;
    public HashSet<GenrePreferenceEntity> GenrePreferences { get; private set; } = [];

    HashSet<GenrePreferenceEntity> IHasGenreJoins<GenrePreferenceEntity>.GenreJoins => GenrePreferences;

    public static PreferenceEntity Create(Guid userId, double radiusKm, IEnumerable<int> genreIds)
    {
        var preference = new PreferenceEntity { UserId = userId, RadiusKm = radiusKm };
        preference.SyncGenres(genreIds);
        return preference;
    }

    public void Update(double radiusKm, IEnumerable<int> genreIds)
    {
        RadiusKm = radiusKm;
        SyncGenres(genreIds);
    }

    public void SyncGenres(IEnumerable<int> genreIds) =>
        this.SyncGenres<GenrePreferenceEntity>(genreIds);
}
