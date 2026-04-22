using Bogus;
using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.Seeding.Fakers;

public static class ArtistFaker
{
    public static Faker<ArtistEntity> GetFaker(Guid userId, string name, string bannerUrl)
    {
        return new Faker<ArtistEntity>()
            .CustomInstantiator(_ => New<ArtistEntity>())
            .RuleFor(a => a.UserId, userId)
            .RuleFor(a => a.Name, name)
            .RuleFor(a => a.BannerUrl, bannerUrl)
            .RuleFor(a => a.About, f => f.Lorem.Paragraph(7));
    }
}
