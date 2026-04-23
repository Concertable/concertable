using Concertable.Concert.Application.Interfaces.Reviews;
using Concertable.Concert.Infrastructure.Data;
using Concertable.Concert.Infrastructure.Mappers;
using Concertable.Infrastructure.Helpers;
using Concertable.Shared;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Repositories.Review;

internal class ConcertReviewRepository(ConcertDbContext context, TimeProvider timeProvider)
    : IConcertReviewRepository
{
    public Task<IPagination<ReviewDto>> GetByConcertAsync(int concertId, IPageParams pageParams) =>
        context.Reviews
            .AsNoTracking()
            .Where(r => r.Ticket.ConcertId == concertId)
            .OrderByDescending(r => r.Id)
            .ToDto()
            .ToPaginationAsync(pageParams);

    public async Task<ReviewSummaryDto> GetSummaryByConcertAsync(int concertId)
    {
        var projection = await context.ConcertRatingProjections
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ConcertId == concertId);
        return projection is null
            ? new ReviewSummaryDto(0, null)
            : new ReviewSummaryDto(projection.ReviewCount, projection.AverageRating);
    }

    public Task<bool> CanUserReviewConcertAsync(Guid userId, int concertId) =>
        context.Tickets
            .AsNoTracking()
            .AnyAsync(t =>
                t.UserId == userId &&
                t.Review == null &&
                t.ConcertId == concertId &&
                t.Concert.Booking.Application.Opportunity.Period.Start <= timeProvider.GetUtcNow());

    public async Task<ReviewEntity> AddAsync(ReviewEntity review)
    {
        await context.Reviews.AddAsync(review);
        return review;
    }

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
