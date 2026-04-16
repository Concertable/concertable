using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Exceptions;
using Concertable.Core.Extensions;
using Concertable.Core.Interfaces;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace Concertable.Core.Entities;

public class ArtistEntity : IIdEntity, IHasName, ILocatable<ArtistEntity>, IReviewable<ArtistEntity>, IHasGenreJoins<ArtistGenreEntity>
{
    private ArtistEntity() { }

    public int Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string About { get; private set; } = null!;
    public string BannerUrl { get; private set; } = null!;
    public ArtistManagerEntity User { get; set; } = null!;
    public static Expression<Func<ArtistEntity, Point?>> LocationExpression => a => a.User.Location;
    public static Expression<Func<ReviewEntity, int>> ReviewIdSelector => r => r.Ticket.Concert.Application.ArtistId;

    public HashSet<ArtistGenreEntity> ArtistGenres { get; private set; } = [];
    public HashSet<OpportunityApplicationEntity> Applications { get; private set; } = [];

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
