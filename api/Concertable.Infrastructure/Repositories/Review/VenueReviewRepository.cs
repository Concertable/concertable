using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;

namespace Concertable.Infrastructure.Repositories.Review;

public class VenueReviewRepository : IVenueReviewRepository
{
    private readonly IReviewRepository<VenueEntity> reviewRepository;

    public VenueReviewRepository(IReviewRepository<VenueEntity> reviewRepository)
    {
        this.reviewRepository = reviewRepository;
    }

    public Task<IPagination<ReviewDto>> GetAsync(int id, IPageParams pageParams) =>
        reviewRepository.GetAsync(id, pageParams);

    public Task<ReviewSummaryDto> GetSummaryAsync(int id) =>
        reviewRepository.GetSummaryAsync(id);

    public Task<bool> CanReviewAsync(Guid userId, int id) =>
        throw new NotImplementedException();
}
