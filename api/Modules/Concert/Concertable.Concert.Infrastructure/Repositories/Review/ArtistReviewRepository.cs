using Concertable.Concert.Application.Interfaces.Reviews;
using Concertable.Concert.Infrastructure.Data;
using Concertable.Concert.Infrastructure.Mappers;
using Concertable.Shared;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Repositories.Review;

internal class ArtistReviewRepository(ConcertDbContext context, TimeProvider timeProvider)
    : IArtistReviewRepository
{
    public Task<IPagination<ReviewDto>> GetByArtistAsync(int artistId, IPageParams pageParams) =>
        context.Reviews
            .AsNoTracking()
            .Where(r => r.Ticket.Concert.Booking.Application.ArtistId == artistId)
            .OrderByDescending(r => r.Id)
            .ToDto()
            .ToPaginationAsync(pageParams);

    public Task<bool> CanUserReviewArtistAsync(Guid userId, int artistId) =>
        context.Tickets
            .AsNoTracking()
            .AnyAsync(t =>
                t.UserId == userId &&
                t.Review == null &&
                t.Concert.Booking.Application.ArtistId == artistId &&
                t.Concert.Booking.Application.Opportunity.Period.Start <= timeProvider.GetUtcNow());
}
