using Core.Entities;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using AutoMapper;
using Core.Entities.Identity;
using System.Runtime.InteropServices;
using Core.Exceptions;

namespace Infrastructure.Services
{
    public class ListingService: IListingService
    {
        private readonly IListingRepository listingRepository;
        private readonly IStripeValidationService stripeValidationService;
        private readonly IVenueService venueService;
        private readonly IMapper mapper;

        public ListingService(
            IListingRepository listingRepository, 
            IStripeValidationService stripeValidationService,
            IVenueService venueService, 
            IMapper mapper)
        {
            this.listingRepository = listingRepository;
            this.stripeValidationService = stripeValidationService;
            this.venueService = venueService;
            this.mapper = mapper;
        }

        public async Task CreateAsync(ListingDto listingDto)
        {
            await stripeValidationService.ValidateUserAsync();

            var venueDto = await venueService.GetDetailsForCurrentUserAsync(); // Fetch venue once
            var listing = MapGenres(listingDto, venueDto); // Use the MapToListing method

            await listingRepository.AddAsync(listing);
        }

        public async Task CreateMultipleAsync(IEnumerable<ListingDto> listingsDto)
        {
            await stripeValidationService.ValidateUserAsync();

            var venueDto = await venueService.GetDetailsForCurrentUserAsync(); // Fetch venue once

            // Use MapToListing for each listing
            var listings = listingsDto.Select(dto => MapGenres(dto, venueDto)).ToList(); // Map all listings

            await listingRepository.AddRangeAsync(listings);
            await listingRepository.SaveChangesAsync();
        }

        private Listing MapGenres(ListingDto listingDto, VenueDto venueDto)
        {
            var listing = mapper.Map<Listing>(listingDto); // AutoMapper handles basic mapping

            listing.VenueId = venueDto.Id; // Set VenueId manually

            // Map ListingGenres
            listing.ListingGenres = listingDto.Genres
                .Select(g => new ListingGenre { GenreId = g.Id })
                .ToList();

            return listing;
        }

        public async Task<IEnumerable<ListingDto>> GetActiveByVenueIdAsync(int id)
        {
            var listings = await listingRepository.GetActiveByVenueIdAsync(id);
            return mapper.Map<IEnumerable<ListingDto>>(listings);
        }

        public async Task<Listing> GetByIdAsync(int id)
        {
            return await listingRepository.GetByIdAsync(id);
        }

        public async Task<VenueManager> GetOwnerByIdAsync(int id)
        {
            return await listingRepository.GetOwnerByIdAsync(id); 
        }
    }
}
