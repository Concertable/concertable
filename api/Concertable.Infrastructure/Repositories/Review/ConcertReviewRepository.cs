using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Results;
using Concertable.Core.Entities;
using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;
using Concertable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories.Review;

public class ConcertReviewRepository : IConcertReviewRepository
{
    private readonly IReviewRepository<ConcertEntity> reviewRepository;
    private readonly ApplicationDbContext context;
    private readonly TimeProvider timeProvider;

    public ConcertReviewRepository(
        IReviewRepository<ConcertEntity> reviewRepository,
        ApplicationDbContext context,
        TimeProvider timeProvider)
    {
        this.reviewRepository = reviewRepository;
        this.context = context;
        this.timeProvider = timeProvider;
    }

    public Task<IPagination<ReviewDto>> GetAsync(int id, IPageParams pageParams) =>
        reviewRepository.GetAsync(id, pageParams);

    public Task<ReviewSummaryDto> GetSummaryAsync(int id) =>
        reviewRepository.GetSummaryAsync(id);

    public Task<bool> CanReviewAsync(Guid userId, int id) =>
        context.Tickets
            .Where(t => t.UserId == userId && t.Review == null)
            .AnyAsync(t => t.ConcertId == id && t.Concert.Application.Opportunity.StartDate <= timeProvider.GetUtcNow());

    public async Task<ReviewEntity> AddAsync(ReviewEntity review)
    {
        await context.Reviews.AddAsync(review);
        return review;
    }

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
