using Bogus;
using Concertable.Core.Entities;

namespace Concertable.Seeding.Fakers;

public static class ConcertFaker
{
    public static Faker<ConcertEntity> GetFaker(int applicationId, string name, decimal price, int totalTickets, int availableTickets, DateTime datePosted)
    {
        return new Faker<ConcertEntity>()
            .CustomInstantiator(_ => (ConcertEntity)Activator.CreateInstance(typeof(ConcertEntity), nonPublic: true)!)
            .RuleFor(e => e.ApplicationId, applicationId)
            .RuleFor(e => e.Name, f => name)
            .RuleFor(e => e.About, f => f.Lorem.Paragraph(7))
            .RuleFor(e => e.Price, price)
            .RuleFor(e => e.TotalTickets, totalTickets)
            .RuleFor(e => e.AvailableTickets, availableTickets)
            .RuleFor(e => e.DatePosted, datePosted);
    }
}
