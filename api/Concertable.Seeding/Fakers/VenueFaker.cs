using Bogus;
using Concertable.Venue.Domain;

namespace Concertable.Seeding.Fakers;

public static class VenueFaker
{
    public static Faker<VenueEntity> GetFaker(Guid userId, string name, string bannerUrl)
    {
        return new Faker<VenueEntity>()
            .CustomInstantiator(f => VenueEntity.Create(userId, name, f.Lorem.Paragraph(7), bannerUrl))
            .RuleFor(v => v.Approved, true);
    }
}
