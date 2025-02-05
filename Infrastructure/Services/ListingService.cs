using Core.Entities;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ListingService: IListingService
    {
        private readonly IListingRepository listingRepository;
        private readonly IVenueService venueService;

        public ListingService(IListingRepository listingRepository, IVenueService venueService)
        {
            this.listingRepository = listingRepository;
            this.venueService = venueService;
        }

        public async void Create(Listing listing)
        {
            var venue = await venueService.GetUserVenueAsync();
            listing.VenueId = venue.Id;

            listingRepository.Add(listing);
        }

        public async Task<IEnumerable<Listing>> GetActiveByVenueIdAsync(int id)
        {
            return await listingRepository.GetActiveByVenueIdAsync(id);
        }
    }
}
