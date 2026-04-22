using Concertable.Concert.Infrastructure.Data;
using Concertable.Infrastructure.Helpers;
using Concertable.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Repositories.Review;

internal class VenueReviewRepository : IVenueReviewRepository
{
    private readonly ConcertDbContext context;

    public VenueReviewRepository(ConcertDbContext context)
    {
        this.context = context;
    }

    public Task<IPagination<ReviewDto>> GetAsync(int id, IPageParams pageParams) =>
        context.Reviews
            .Where(r => r.Ticket.Concert.Booking.Application.Opportunity.VenueId == id)
            .OrderByDescending(r => r.Id)
            .ToDto()
            .ToPaginationAsync(pageParams);

    public async Task<ReviewSummaryDto> GetSummaryAsync(int id) =>
        await context.Reviews
            .Where(r => r.Ticket.Concert.Booking.Application.Opportunity.VenueId == id)
            .ToSummaryDto()
            .FirstOrDefaultAsync()
            ?? new ReviewSummaryDto(0, null);

    public Task<bool> CanReviewAsync(Guid userId, int id) =>
        throw new NotImplementedException();
}
