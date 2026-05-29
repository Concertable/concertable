using Bogus;
using Concertable.B2B.Artist.Domain;
using Concertable.Contracts;
using Concertable.Kernel;
using NetTopologySuite.Geometries;
using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.B2B.Seeding.Fakers;

public static class ArtistFaker
{
    private static readonly Faker faker = new();

    public static ArtistEntity Create(
        int id,
        Guid userId,
        string name,
        string bannerUrl,
        string avatar,
        Point location,
        Address address,
        string email,
        IEnumerable<Genre> genres)
        => ArtistEntity
            .Create(userId, name, faker.Lorem.Paragraph(7), bannerUrl, avatar, location, address, email, genres)
            .With(nameof(ArtistEntity.Id), id);
}
