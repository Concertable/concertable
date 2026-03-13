using Application.Interfaces;
using Application.Responses;

namespace Infrastructure.Validators;

public class ListingApplicationValidator : IListingApplicationValidator
{
    private readonly IConcertRepository concertRepository;
    private readonly IListingRepository listingRepository;
    private readonly IListingApplicationRepository listingApplicationRepository;
    private readonly ICurrentUser currentUser;
    private readonly TimeProvider timeProvider;

    public ListingApplicationValidator(
        IConcertRepository concertRepository,
        IListingRepository listingRepository,
        IListingApplicationRepository listingApplicationRepository,
        ICurrentUser currentUser,
        TimeProvider timeProvider)
    {
        this.concertRepository = concertRepository;
        this.listingRepository = listingRepository;
        this.listingApplicationRepository = listingApplicationRepository;
        this.currentUser = currentUser;
        this.timeProvider = timeProvider;
    }

    public async Task<ValidationResult> CanAcceptListingApplicationAsync(int applicationId)
    {
        var result = new ValidationResult();
        var listing = await listingRepository.GetByApplicationIdAsync(applicationId);
        var listingApplication = await listingApplicationRepository.GetByIdAsync(applicationId);

        if (listing is null)
            return result.AddError("Listing does not exist");

        if (listingApplication is null)
            return result.AddError("Listing Application does not exist");

        var userId = currentUser.Get().Id;
        if (listing.Venue.UserId != userId)
            return result.AddError("You do not own this Listing");

        if (listing.StartDate < timeProvider.GetUtcNow())
            return result.AddError("You can't accept this application because the listing has already passed");

        if (await concertRepository.ListingHasConcertAsync(listing.Id))
            return result.AddError("This listing already has a concert booked");

        if (await concertRepository.ArtistHasConcertOnDateAsync(listingApplication.ArtistId, listing.StartDate))
            return result.AddError("This artist already has a concert on this day");

        if (await concertRepository.VenueHasConcertOnDateAsync(listing.VenueId, listing.StartDate))
            return result.AddError("You already have a concert on this day");

        return result;
    }

    public async Task<ValidationResult> CanApplyForListingAsync(int listingId, int artistId)
    {
        var result = new ValidationResult();
        var listing = await listingRepository.GetByIdAsync(listingId);

        if (listing is null)
            return result.AddError("Listing does not exist.");

        if (listing.StartDate < timeProvider.GetUtcNow())
            return result.AddError("This listing has already passed.");

        if (await concertRepository.ListingHasConcertAsync(listingId))
            return result.AddError("This listing has already been booked for a concert.");

        if (await concertRepository.ArtistHasConcertOnDateAsync(artistId, listing.StartDate))
            return result.AddError("You already have a concert on this day.");

        return result;
    }
}
