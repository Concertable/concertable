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

namespace Infrastructure.Services
{
    public class ListingService: IListingService
    {
        private readonly IListingRepository listingRepository;
        private readonly IVenueService venueService;
        private readonly IMapper mapper;

        public ListingService(IListingRepository listingRepository, IVenueService venueService, IMapper mapper)
        {
            this.listingRepository = listingRepository;
            this.venueService = venueService;
            this.mapper = mapper;
        }

        public async Task CreateAsync(ListingDto listingDto)
        {
            var listing = mapper.Map<Listing>(listingDto);

            var venueDto = await venueService.GetDetailsForCurrentUserAsync();
            listing.VenueId = venueDto.Id;

            await listingRepository.AddAsync(listing);
        }

        public async Task<IEnumerable<ListingDto>> GetActiveByVenueIdAsync(int id)
        {
            var listings = await listingRepository.GetActiveByVenueIdAsync(id);
            return mapper.Map<IEnumerable<ListingDto>>(listings);
        }

        public async Task<VenueManager> GetOwnerByIdAsync(int id)
        {
            return await listingRepository.GetOwnerByIdAsync(id); 
        }
    }
}
