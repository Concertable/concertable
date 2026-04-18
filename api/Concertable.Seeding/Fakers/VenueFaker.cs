using Bogus;
using Concertable.Core.Entities;
using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.Seeding.Fakers;

public static class VenueFaker
{
    public static Faker<VenueEntity> GetFaker(Guid userId, string name, string bannerUrl)
    {
        return new Faker<VenueEntity>()
            .CustomInstantiator(_ => New<VenueEntity>())
            .RuleFor(v => v.UserId, userId)
            .RuleFor(v => v.Name, name)
            .RuleFor(v => v.BannerUrl, bannerUrl)
            .RuleFor(v => v.About, f => f.Lorem.Paragraph(7))
            .RuleFor(v => v.Approved, true);
    }
}
