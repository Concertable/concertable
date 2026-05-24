using Bogus;
using Concertable.B2B.Concert.Domain.Entities;
using Concertable.Seeding.Extensions;

namespace Concertable.Seeding.Fakers;

public static class ConcertFaker
{
    public static Faker<ConcertEntity> GetFaker(
        int id,
        int bookingId,
        string name,
        decimal price,
        int totalTickets,
        int artistId,
        int venueId,
        DateTime startDate,
        DateTime endDate,
        IEnumerable<Genre>? genres = null)
    {
        return new Faker<ConcertEntity>()
            .CustomInstantiator(f => ConcertEntity
                .CreateDraft(bookingId, artistId, venueId, new DateRange(startDate, endDate), name, f.Lorem.Paragraph(7), genres ?? [])
                .With("Id", id)
                .With("Price", price)
                .With("TotalTickets", totalTickets));
    }
}
