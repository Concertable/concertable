using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Responses;
using Concertable.Core.Parameters;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Helpers;
using Concertable.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories.Review;

public class ConcertReviewRepository : IConcertReviewRepository
{
    private readonly ApplicationDbContext context;
    private readonly TimeProvider timeProvider;

    public ConcertReviewRepository(
        ApplicationDbContext context,
        TimeProvider timeProvider)
    {
        this.context = context;
        this.timeProvider = timeProvider;
    }

    public Task<IPagination<ReviewDto>> GetAsync(int id, IPageParams pageParams) =>
        context.Reviews
            .Where(r => r.Ticket.ConcertId == id)
            .OrderByDescending(r => r.Id)
            .ToDto()
            .ToPaginationAsync(pageParams);

    public async Task<ReviewSummaryDto> GetSummaryAsync(int id) =>
        await context.Reviews
            .Where(r => r.Ticket.ConcertId == id)
            .ToSummaryDto()
            .FirstOrDefaultAsync()
            ?? new ReviewSummaryDto(0, null);

    public Task<bool> CanReviewAsync(Guid userId, int id) =>
        context.Tickets
            .Where(t => t.UserId == userId && t.Review == null)
            .AnyAsync(t => t.ConcertId == id && t.Concert.Booking.Application.Opportunity.Period.Start <= timeProvider.GetUtcNow());

    public async Task<ReviewEntity> AddAsync(ReviewEntity review)
    {
        await context.Reviews.AddAsync(review);
        return review;
    }

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
