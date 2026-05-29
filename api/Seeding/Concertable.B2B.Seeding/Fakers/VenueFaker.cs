using Bogus;
using Concertable.B2B.Venue.Domain;
using Concertable.Kernel;
using NetTopologySuite.Geometries;
using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.B2B.Seeding.Fakers;

public static class VenueFaker
{
    private static readonly Faker faker = new();

    public static VenueEntity Create(
        int id,
        Guid userId,
        string name,
        string bannerUrl,
        string avatar,
        Point location,
        Address address,
        string email)
    {
        var venue = VenueEntity
            .Create(userId, name, faker.Lorem.Paragraph(7), bannerUrl, avatar, location, address, email)
            .With(nameof(VenueEntity.Id), id);
        venue.Approve();
        return venue;
    }
}
