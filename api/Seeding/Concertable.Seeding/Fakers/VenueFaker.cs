using Bogus;
using Concertable.Venue.Domain;
using NetTopologySuite.Geometries;

namespace Concertable.Seeding.Fakers;

public static class VenueFaker
{
    public static Faker<VenueEntity> GetFaker(
        Guid userId,
        string name,
        string bannerUrl,
        string avatar,
        Point location,
        Address address,
        string email)
    {
        return new Faker<VenueEntity>()
            .CustomInstantiator(f => VenueEntity.Create(userId, name, f.Lorem.Paragraph(7), bannerUrl, avatar, location, address, email))
            .FinishWith((_, v) => v.Approve());
    }
}
