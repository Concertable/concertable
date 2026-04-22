using NetTopologySuite.Geometries;

namespace Concertable.Artist.Domain;

public class ArtistEntity : IIdEntity, IHasName, IHasLocation, IHasGenreJoins<ArtistGenreEntity>
{
    private ArtistEntity() { }

    public int Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string About { get; private set; } = null!;
    public string BannerUrl { get; private set; } = null!;
    public Point? Location { get; set; }
    public Address? Address { get; set; }
    public string? Avatar { get; set; }
    public string? Email { get; set; }

    public HashSet<ArtistGenreEntity> ArtistGenres { get; private set; } = [];

    HashSet<ArtistGenreEntity> IHasGenreJoins<ArtistGenreEntity>.GenreJoins => ArtistGenres;

    public static ArtistEntity Create(Guid userId, string name, string about, string bannerUrl, IEnumerable<int> genreIds)
    {
        ValidateFields(name, about, bannerUrl);

        var artist = new ArtistEntity
        {
            UserId = userId,
            Name = name,
            About = about,
            BannerUrl = bannerUrl
        };

        artist.SyncGenres(genreIds);

        return artist;
    }

    public void Update(string name, string about, string bannerUrl, IEnumerable<int> genreIds)
    {
        ValidateFields(name, about, bannerUrl);

        Name = name;
        About = about;
        BannerUrl = bannerUrl;

        SyncGenres(genreIds);
    }

    public void SyncGenres(IEnumerable<int> genreIds) =>
        this.SyncGenres<ArtistGenreEntity>(genreIds);

    private static void ValidateFields(string name, string about, string bannerUrl)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required.");
        if (string.IsNullOrWhiteSpace(about)) throw new DomainException("About is required.");
        if (string.IsNullOrWhiteSpace(bannerUrl)) throw new DomainException("Banner URL is required.");
    }
}
