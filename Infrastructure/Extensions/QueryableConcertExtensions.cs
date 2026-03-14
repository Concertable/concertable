using Core.Entities;

namespace Infrastructure.Extensions;

public static class ConcertQueryableExtensions
{
    public static IQueryable<Concert> Active(this IQueryable<Concert> query, DateTime now) =>
        query.Where(c => c.DatePosted != null && c.Application.Opportunity.EndDate > now);
}
