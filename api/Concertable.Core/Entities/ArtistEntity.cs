using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Interfaces;
using NetTopologySuite.Geometries;

namespace Concertable.Core.Entities;

public class ArtistEntity : IEntity, IHasName, IHasLocation
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public required string About { get; set; }
    public required string BannerUrl { get; set; }
    public ArtistManagerEntity User { get; set; } = null!;
    public Point? Location => User.Location;
    public ICollection<ArtistGenreEntity> ArtistGenres { get; set; } = new List<ArtistGenreEntity>();
    public ICollection<SocialMediaEntity> SocialMedias { get; } = new List<SocialMediaEntity>();
    public ICollection<OpportunityApplicationEntity> Applications { get; } = new List<OpportunityApplicationEntity>();
    public ICollection<VideoEntity> Videos { get; } = new List<VideoEntity>();
}
