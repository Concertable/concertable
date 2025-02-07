using Core.Entities;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using AutoMapper;

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

        public async void Create(ListingDto listingDto)
        {
            var listing = mapper.Map<Listing>(listingDto);

            var venue = await venueService.GetUserVenueAsync();
            listing.VenueId = venue.Id;

            listingRepository.Add(listing);
        }

        public async Task<IEnumerable<ListingDto>> GetActiveByVenueIdAsync(int id)
        {
            var listings = await listingRepository.GetActiveByVenueIdAsync(id);
            return mapper.Map<IEnumerable<ListingDto>>(listings);
        }
    }
}
