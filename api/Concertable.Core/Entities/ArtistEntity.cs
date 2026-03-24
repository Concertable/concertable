using Core.Entities.Interfaces;
using Core.Interfaces;
using NetTopologySuite.Geometries;

namespace Core.Entities;

public class ArtistEntity : IEntity, IHasName, IHasLocation
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public required string About { get; set; }
    public required string ImageUrl { get; set; }
    public ArtistManagerEntity User { get; set; } = null!;
    public Point? Location => User.Location;
    public ICollection<ArtistGenreEntity> ArtistGenres { get; set; } = new List<ArtistGenreEntity>();
    public ICollection<SocialMediaEntity> SocialMedias { get; } = new List<SocialMediaEntity>();
    public ICollection<ConcertApplicationEntity> Applications { get; } = new List<ConcertApplicationEntity>();
    public ICollection<VideoEntity> Videos { get; } = new List<VideoEntity>();
}
