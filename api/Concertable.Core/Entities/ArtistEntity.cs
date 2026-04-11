using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Interfaces;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace Concertable.Core.Entities;

public class ArtistEntity : IIdEntity, IHasName, ILocatable<ArtistEntity>, IReviewable<ArtistEntity>
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public required string About { get; set; }
    public required string BannerUrl { get; set; }
    public ArtistManagerEntity User { get; set; } = null!;
    public static Expression<Func<ArtistEntity, Point?>> LocationExpression => a => a.User.Location;
    public static Expression<Func<ReviewEntity, int>> ReviewIdSelector => r => r.Ticket.Concert.Application.ArtistId;
    public ICollection<ArtistGenreEntity> ArtistGenres { get; set; } = [];
    public ICollection<SocialMediaEntity> SocialMedias { get; } = [];
    public ICollection<OpportunityApplicationEntity> Applications { get; } = [];
    public ICollection<VideoEntity> Videos { get; } = [];
}
