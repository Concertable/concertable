using Application.Interfaces;
using Core.Interfaces;

namespace Infrastructure.Validators;

public class ReviewValidator : IReviewValidator
{
    private readonly IReviewRepository reviewRepository;
    private readonly ICurrentUser currentUser;

    public ReviewValidator(IReviewRepository reviewRepository, ICurrentUser currentUser)
    {
        this.reviewRepository = reviewRepository;
        this.currentUser = currentUser;
    }

    public async Task<bool> CanUserReviewConcertAsync(int concertId) =>
        await reviewRepository.CanUserIdReviewConcertIdAsync(currentUser.GetId(), concertId);

    public async Task<bool> CanUserReviewVenueAsync(int venueId) =>
        await reviewRepository.CanUserIdReviewVenueIdAsync(currentUser.GetId(), venueId);

    public async Task<bool> CanUserReviewArtistAsync(int artistId) =>
        await reviewRepository.CanUserIdReviewArtistIdAsync(currentUser.GetId(), artistId);
}
