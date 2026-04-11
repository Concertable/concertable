using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Requests;
using Concertable.Application.Results;
using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;

namespace Concertable.Infrastructure.Services.Review;

public class VenueReviewService : IReviewService
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
