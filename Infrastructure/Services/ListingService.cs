using Core.Entities;
using Application.Interfaces;
using Application.DTOs;
using Application.Mappers;
using Core.Entities.Identity;
using Core.Exceptions;

namespace Infrastructure.Services
{
    public class ListingService : IListingService
    {
        private readonly IListingRepository listingRepository;
        private readonly IStripeValidationService stripeValidationService;
        private readonly IVenueService venueService;

        public ListingService(
            IListingRepository listingRepository,
            IStripeValidationService stripeValidationService,
            IVenueService venueService)
        {
            this.listingRepository = listingRepository;
            this.stripeValidationService = stripeValidationService;
            this.venueService = venueService;
        }

        public async Task CreateAsync(ListingDto listingDto)
        {
            await stripeValidationService.ValidateUserAsync();

            var venueDto = await venueService.GetDetailsForCurrentUserAsync()
                ?? throw new NotFoundException("Venue not found for current user");
            var listing = listingDto.ToEntity();
            listing.VenueId = venueDto.Id;

            await listingRepository.AddAsync(listing);
            await listingRepository.SaveChangesAsync();
        }

        public async Task CreateMultipleAsync(IEnumerable<ListingDto> listingsDto)
        {
            await stripeValidationService.ValidateUserAsync();

            var venueDto = await venueService.GetDetailsForCurrentUserAsync()
                ?? throw new NotFoundException("Venue not found for current user");

            var listings = listingsDto.Select(dto =>
            {
                var listing = dto.ToEntity();
                listing.VenueId = venueDto.Id;
                return listing;
            }).ToList();

            await listingRepository.AddRangeAsync(listings);
            await listingRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<ListingDto>> GetActiveByVenueIdAsync(int id)
        {
            var listings = await listingRepository.GetActiveByVenueIdAsync(id);
            return listings.ToDtos();
        }

        public async Task<Listing> GetByIdAsync(int id)
        {
            return (await listingRepository.GetByIdAsync(id))!;
        }

        public async Task<VenueManager> GetOwnerByIdAsync(int id)
        {
            return await listingRepository.GetOwnerByIdAsync(id);
        }
    }
}
