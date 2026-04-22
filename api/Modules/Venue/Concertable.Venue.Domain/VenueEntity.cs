using NetTopologySuite.Geometries;

namespace Concertable.Venue.Domain;

public class VenueEntity : IIdEntity, IHasName, IHasLocation
{
    private VenueEntity() { }

    public int Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string About { get; private set; } = null!;
    public string BannerUrl { get; private set; } = null!;
    public bool Approved { get; private set; }
    public Point? Location { get; set; }
    public Address? Address { get; set; }
    public string? Avatar { get; set; }
    public string? Email { get; set; }

    public static VenueEntity Create(Guid userId, string name, string about, string bannerUrl)
    {
        ValidateFields(name, about, bannerUrl);
        return new() { UserId = userId, Name = name, About = about, BannerUrl = bannerUrl };
    }

    public void Update(string name, string about, string bannerUrl)
    {
        ValidateFields(name, about, bannerUrl);
        Name = name;
        About = about;
        BannerUrl = bannerUrl;
    }

    public void Approve() => Approved = true;

    private static void ValidateFields(string name, string about, string bannerUrl)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required.");
        if (string.IsNullOrWhiteSpace(about)) throw new DomainException("About is required.");
        if (string.IsNullOrWhiteSpace(bannerUrl)) throw new DomainException("Banner URL is required.");
    }
}
