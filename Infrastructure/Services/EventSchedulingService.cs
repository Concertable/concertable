using Application.Interfaces;
using Application.Responses;

namespace Infrastructure.Services
{
    public class EventSchedulingService : IEventSchedulingService
    {
        private readonly IEventRepository eventRepository;
        private readonly IListingRepository listingRepository;
        private readonly IListingApplicationRepository listingApplicationRepository;
        private readonly ICurrentUserService currentUserService;

        public EventSchedulingService(
            IEventRepository eventRepository, 
            IListingRepository listingRepository, 
            IListingApplicationRepository listingApplicationRepository,
            ICurrentUserService currentUserService)
        {
            this.eventRepository = eventRepository;
            this.listingRepository = listingRepository;
            this.listingApplicationRepository = listingApplicationRepository;
            this.currentUserService = currentUserService;
            
        }

        public async Task<ValidationResponse> CanAcceptListingApplicationAsync(int applicationId)
        {
            var listing = await listingRepository.GetByApplicationIdAsync(applicationId);
            var listingApplication = await listingApplicationRepository.GetByIdAsync(applicationId);
            var user = await currentUserService.GetEntityAsync();

            if(listing is null)
                return ValidationResponse.Failure("Listing does not exist");

            if (listingApplication is null)
                return ValidationResponse.Failure("Listing Application does not exist");

            if (listing.Venue.UserId != user.Id)
                return ValidationResponse.Failure("You do not own this Listing");

            if (listing.StartDate < DateTime.UtcNow)
                return ValidationResponse.Failure("You can't accept this application because the listing has already passed");

            if (await ListingHasEventAsync(listing.Id))
                return ValidationResponse.Failure("This listing already has an event booked");

            if (await ArtistHasEventOnDateAsync(listingApplication.ArtistId, listing.StartDate))
                return ValidationResponse.Failure("This artist already has an event on this day");

            return ValidationResponse.Success();
        }


        public async Task<ValidationResponse> CanApplyForListingAsync(int listingId, int artistId)
        {
            var listing = await listingRepository.GetByIdAsync(listingId);

            if (listing is null)
                return ValidationResponse.Failure("Listing does not exist.");

            if (listing.StartDate < DateTime.UtcNow)
                return ValidationResponse.Failure("This listing has already passed.");

            if (await ListingHasEventAsync(listingId))
                return ValidationResponse.Failure("This listing has already been booked for an event.");

            if (await ArtistHasEventOnDateAsync(artistId, listing.StartDate))
                return ValidationResponse.Failure("You already have an event on this day.");

            return ValidationResponse.Success();
        }


        private Task<bool> ArtistHasEventOnDateAsync(int artistId, DateTime date)
        {
            return eventRepository.ArtistHasEventOnDateAsync(artistId, date);
        }

        private Task<bool> ListingHasEventAsync(int listingId)
        {
            return eventRepository.ListingHasEventAsync(listingId);
        }
    }
}
