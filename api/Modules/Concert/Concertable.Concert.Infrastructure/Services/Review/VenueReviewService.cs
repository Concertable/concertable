namespace Concertable.Concert.Infrastructure.Services.Review;

internal class VenueReviewService : IReviewService
{
    private readonly IVenueReviewRepository reviewRepository;

    public VenueReviewService(IVenueReviewRepository reviewRepository)
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
