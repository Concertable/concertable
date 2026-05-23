using Bogus;
using Concertable.Artist.Domain;
using Concertable.Contracts;
using Concertable.Kernel;
using Concertable.Seeding.Extensions;
using NetTopologySuite.Geometries;

namespace Concertable.Seeding.Fakers;

public static class ArtistFaker
{
    public static Faker<ArtistEntity> GetFaker(
        int id,
        Guid userId,
        string name,
        string bannerUrl,
        string avatar,
        Point location,
        Address address,
        string email,
        IEnumerable<Genre> genres)
    {
        return new Faker<ArtistEntity>()
            .CustomInstantiator(f =>
                ArtistEntity.Create(userId, name, f.Lorem.Paragraph(7), bannerUrl, avatar, location, address, email, genres)
                    .With(nameof(ArtistEntity.Id), id));
    }
}
