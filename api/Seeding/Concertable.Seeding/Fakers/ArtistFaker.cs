using Bogus;
using Concertable.Artist.Domain;

namespace Concertable.Seeding.Fakers;

public static class ArtistFaker
{
    public static Faker<ArtistEntity> GetFaker(Guid userId, string name, string bannerUrl)
    {
        return new Faker<ArtistEntity>()
            .CustomInstantiator(f =>
                ArtistEntity.Create(userId, name, f.Lorem.Paragraph(7), bannerUrl, Array.Empty<int>()));
    }
}
