using Concertable.Venue.Domain.Events;
using NetTopologySuite.Geometries;

namespace Concertable.Venue.Domain;

public class VenueEntity : IIdEntity, IHasName, IHasLocation, IEventRaiser
{
    private readonly EventRaiser _events = new();

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

    public IReadOnlyList<IDomainEvent> DomainEvents => _events.DomainEvents;
    public void ClearDomainEvents() => _events.Clear();

    public static VenueEntity Create(Guid userId, string name, string about, string bannerUrl)
    {
        ValidateFields(name, about, bannerUrl);
        var venue = new VenueEntity { UserId = userId, Name = name, About = about, BannerUrl = bannerUrl };
        venue._events.Raise(new VenueChangedDomainEvent(venue));
        return venue;
    }

    public void Update(string name, string about, string bannerUrl)
    {
        ValidateFields(name, about, bannerUrl);
        Name = name;
        About = about;
        BannerUrl = bannerUrl;
        _events.Raise(new VenueChangedDomainEvent(this));
    }

    public void Approve()
    {
        Approved = true;
        _events.Raise(new VenueChangedDomainEvent(this));
    }

    private static void ValidateFields(string name, string about, string bannerUrl)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required.");
        if (string.IsNullOrWhiteSpace(about)) throw new DomainException("About is required.");
        if (string.IsNullOrWhiteSpace(bannerUrl)) throw new DomainException("Banner URL is required.");
    }
}
