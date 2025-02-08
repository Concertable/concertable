using Core.Entities;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ListingApplicationService : IListingApplicationService
    {
        private readonly IRegisterRepository registerRepository;
        private readonly IArtistService artistService;

        public ListingApplicationService(IRegisterRepository registerRepository, IArtistService artistService)
        {
            this.registerRepository = registerRepository;
            this.artistService = artistService;
        }

        public async Task<IEnumerable<ListingApplication>> GetAllForListingIdAsync(int listingId)
        {
            return await registerRepository.GetAllForListingIdAsync(listingId);
        }

        public async Task RegisterForListingAsync(int listingId)
        {
            var artistDto = await artistService.GetDetailsForCurrentUserAsync();
            var listing = new ListingApplication()
            {
                ListingId = listingId,
                ArtistId = artistDto.Id,
                Approved = false
            };

            await registerRepository.AddAsync(listing);
        }
    }
}
