using Concertable.Core.Entities;

namespace Concertable.Infrastructure.Extensions;

public static class ConcertQueryableExtensions
{
    public static IQueryable<ConcertEntity> Active(this IQueryable<ConcertEntity> query, DateTime now) =>
        query.Where(c => c.DatePosted != null && c.Booking.Application.Opportunity.Period.End > now);
}
