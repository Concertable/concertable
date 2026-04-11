using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Results;
using Concertable.Core.Entities;
using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;

namespace Concertable.Infrastructure.Repositories.Review;

public class ArtistReviewRepository : IArtistReviewRepository
{
    private readonly IReviewRepository<ArtistEntity> reviewRepository;

    public ArtistReviewRepository(IReviewRepository<ArtistEntity> reviewRepository)
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
