using Bogus;
using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.Seeding.Fakers;

public static class ConcertFaker
{
    public static Faker<ConcertEntity> GetFaker(
        string name,
        decimal price,
        int totalTickets,
        int availableTickets,
        int artistId,
        int venueId,
        DateTime startDate,
        DateTime endDate,
        DateTime? datePosted = null)
    {
        var faker = new Faker<ConcertEntity>()
            .CustomInstantiator(_ => New<ConcertEntity>())
            .RuleFor(e => e.Name, f => name)
            .RuleFor(e => e.About, f => f.Lorem.Paragraph(7))
            .RuleFor(e => e.Price, price)
            .RuleFor(e => e.TotalTickets, totalTickets)
            .RuleFor(e => e.AvailableTickets, availableTickets)
            .RuleFor(e => e.ArtistId, artistId)
            .RuleFor(e => e.VenueId, venueId)
            .RuleFor(e => e.StartDate, startDate)
            .RuleFor(e => e.EndDate, endDate);

        if (datePosted.HasValue)
            faker.RuleFor(e => e.DatePosted, datePosted.Value);

        return faker;
    }
}
