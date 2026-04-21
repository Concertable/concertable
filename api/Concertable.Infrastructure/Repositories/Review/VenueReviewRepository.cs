using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Responses;
using Concertable.Core.Parameters;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Helpers;
using Concertable.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories.Review;

public class VenueReviewRepository : IVenueReviewRepository
{
    private readonly ApplicationDbContext context;

    public VenueReviewRepository(ApplicationDbContext context)
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
