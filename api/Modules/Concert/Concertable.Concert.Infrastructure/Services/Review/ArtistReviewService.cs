namespace Concertable.Concert.Infrastructure.Services.Review;

internal class ArtistReviewService : IReviewService
{
    private readonly IArtistReviewRepository reviewRepository;

    public ArtistReviewService(IArtistReviewRepository reviewRepository)
    {
        this.reviewRepository = reviewRepository;
    }

    public Task<IPagination<ReviewDto>> GetAsync(int id, IPageParams pageParams) =>
        reviewRepository.GetAsync(id, pageParams);

    public Task<ReviewSummaryDto> GetSummaryAsync(int id) =>
        reviewRepository.GetSummaryAsync(id);

    public Task<ReviewDto> CreateAsync(CreateReviewRequest request) =>
        throw new NotImplementedException();
}
