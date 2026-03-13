using Core.Entities;
using Core.Interfaces;
using NetTopologySuite.Geometries;

namespace Core.Entities;

public class Artist : BaseEntity, IHasName, IHasLocation
{
    public int UserId { get; set; }
    public required string Name { get; set; }
    public required string About { get; set; }
    public required string ImageUrl { get; set; }
    public ArtistManager User { get; set; } = null!;
    public Point? Location => User.Location;
    public ICollection<ArtistGenre> ArtistGenres { get; set; } = new List<ArtistGenre>();
    public ICollection<SocialMedia> SocialMedias { get; } = new List<SocialMedia>();
    public ICollection<ConcertApplication> Applications { get; } = new List<ConcertApplication>();
    public ICollection<Video> Videos { get; } = new List<Video>();
}
