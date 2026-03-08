using Bogus;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.SeedData;

public static class ConcertFaker
{
    public static Faker<Concert> GetFaker(int applicationId, string name, decimal price, int totalTickets, int availableTickets, DateTime datePosted)
    {
        return new Faker<Concert>()
            .RuleFor(e => e.ApplicationId, applicationId)
            .RuleFor(e => e.Name, f => name)
            .RuleFor(e => e.About, f => f.Lorem.Paragraph(7))
            .RuleFor(e => e.Price, price)
            .RuleFor(e => e.TotalTickets, totalTickets)
            .RuleFor(e => e.AvailableTickets, availableTickets)
            .RuleFor(e => e.DatePosted, datePosted);
    }
}
