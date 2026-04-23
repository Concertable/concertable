using Concertable.Concert.Application.Interfaces.Reviews;
using Concertable.Concert.Infrastructure.Data;
using Concertable.Concert.Infrastructure.Mappers;
using Concertable.Infrastructure.Helpers;
using Concertable.Shared;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Repositories.Review;

internal class VenueReviewRepository(ConcertDbContext context, TimeProvider timeProvider)
    : IVenueReviewRepository
{
    public Task<IPagination<ReviewDto>> GetByVenueAsync(int venueId, IPageParams pageParams) =>
        context.Reviews
            .AsNoTracking()
            .Where(r => r.Ticket.Concert.Booking.Application.Opportunity.VenueId == venueId)
            .OrderByDescending(r => r.Id)
            .ToDto()
            .ToPaginationAsync(pageParams);

    public Task<bool> CanUserReviewVenueAsync(Guid userId, int venueId) =>
        context.Tickets
            .AsNoTracking()
            .AnyAsync(t =>
                t.UserId == userId &&
                t.Review == null &&
                t.Concert.Booking.Application.Opportunity.VenueId == venueId &&
                t.Concert.Booking.Application.Opportunity.Period.Start <= timeProvider.GetUtcNow());
}
