using Bogus;
using Concertable.B2B.Seeding.Extensions;
using Concertable.B2B.Venue.Domain;
using Concertable.Kernel;
using NetTopologySuite.Geometries;

namespace Concertable.B2B.Seeding.Fakers;

public static class VenueFaker
{
    public static Faker<VenueEntity> GetFaker(
        int id,
        Guid userId,
        string name,
        string bannerUrl,
        string avatar,
        Point location,
        Address address,
        string email)
    {
        return new Faker<VenueEntity>()
            .CustomInstantiator(f => VenueEntity
                .Create(userId, name, f.Lorem.Paragraph(7), bannerUrl, avatar, location, address, email)
                .With(nameof(VenueEntity.Id), id))
            .FinishWith((_, v) => v.Approve());
    }
}
