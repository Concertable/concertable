using Concertable.Application.Interfaces;
using Concertable.Core.Interfaces;

namespace Concertable.Infrastructure.Validators;

public class ReviewValidator : IReviewValidator
{
    private readonly IArtistReviewRepository artistReviewRepository;
    private readonly IVenueReviewRepository venueReviewRepository;
    private readonly IConcertReviewRepository concertReviewRepository;
    private readonly ICurrentUser currentUser;

    public ReviewValidator(
        IArtistReviewRepository artistReviewRepository,
        IVenueReviewRepository venueReviewRepository,
        IConcertReviewRepository concertReviewRepository,
        ICurrentUser currentUser)
    {
        this.artistReviewRepository = artistReviewRepository;
        this.venueReviewRepository = venueReviewRepository;
        this.concertReviewRepository = concertReviewRepository;
        this.currentUser = currentUser;
    }

    public Task<bool> CanUserReviewConcertAsync(int concertId) =>
        concertReviewRepository.CanReviewAsync(currentUser.GetId(), concertId);

    public Task<bool> CanUserReviewVenueAsync(int venueId) =>
        venueReviewRepository.CanReviewAsync(currentUser.GetId(), venueId);

    public Task<bool> CanUserReviewArtistAsync(int artistId) =>
        artistReviewRepository.CanReviewAsync(currentUser.GetId(), artistId);
}
