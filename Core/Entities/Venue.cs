using Core.Entities;
using Core.Interfaces;
using NetTopologySuite.Geometries;

namespace Core.Entities;

public class Venue : BaseEntity, IHasName, IHasLocation
{
    public int UserId { get; set; }
    public required string Name { get; set; }
    public required string About { get; set; }
    public required string ImageUrl { get; set; }
    public bool Approved { get; set; }
    public User User { get; set; } = null!;
    public Point? Location => User.Location;
    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
}
