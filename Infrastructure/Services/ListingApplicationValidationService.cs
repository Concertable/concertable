using Application.Interfaces;
using Application.Responses;
using Core.Entities;

namespace Infrastructure.Services;

public class ListingApplicationValidationService : IListingApplicationValidationService
{
    private readonly IConcertRepository concertRepository;
    private readonly IListingRepository listingRepository;
    private readonly IListingApplicationRepository listingApplicationRepository;
    private readonly ICurrentUser currentUser;
    private readonly TimeProvider timeProvider;

    public ListingApplicationValidationService(
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

    public async Task<ValidationResponse> CanAcceptListingApplicationAsync(int applicationId, int userId)
    {
        var listing = await listingRepository.GetByApplicationIdAsync(applicationId);
        var listingApplication = await listingApplicationRepository.GetByIdAsync(applicationId);

        if (listing is null)
            return ValidationResponse.Failure("Listing does not exist");

        if (listingApplication is null)
            return ValidationResponse.Failure("Listing Application does not exist");

        if (listing.Venue.UserId != userId)
            return ValidationResponse.Failure("You do not own this Listing");

        if (listing.StartDate < timeProvider.GetUtcNow())
            return ValidationResponse.Failure("You can't accept this application because the listing has already passed");

        if (await ListingHasConcertAsync(listing.Id))
            return ValidationResponse.Failure("This listing already has a concert booked");

        if (await ArtistHasConcertOnDateAsync(listingApplication.ArtistId, listing.StartDate))
            return ValidationResponse.Failure("This artist already has a concert on this day");

        if (await VenueHasConcertOnDateAsync(listing.VenueId, listing.StartDate))
            return ValidationResponse.Failure("You already have a concert on this day");

        return ValidationResponse.Success();
    }


    public async Task<ValidationResponse> CanApplyForListingAsync(int listingId, int artistId)
    {
        var listing = await listingRepository.GetByIdAsync(listingId);

        if (listing is null)
            return ValidationResponse.Failure("Listing does not exist.");

        if (listing.StartDate < timeProvider.GetUtcNow())
            return ValidationResponse.Failure("This listing has already passed.");

        if (await ListingHasConcertAsync(listingId))
            return ValidationResponse.Failure("This listing has already been booked for a concert.");

        if (await ArtistHasConcertOnDateAsync(artistId, listing.StartDate))
            return ValidationResponse.Failure("You already have a concert on this day.");

        return ValidationResponse.Success();
    }


    private Task<bool> ArtistHasConcertOnDateAsync(int artistId, DateTime date)
    {
        return concertRepository.ArtistHasConcertOnDateAsync(artistId, date);
    }

    private Task<bool> VenueHasConcertOnDateAsync(int venueId, DateTime date)
    {
        return concertRepository.VenueHasConcertOnDateAsync(venueId, date);
    }

    private Task<bool> ListingHasConcertAsync(int listingId)
    {
        return concertRepository.ListingHasConcertAsync(listingId);
    }


}
