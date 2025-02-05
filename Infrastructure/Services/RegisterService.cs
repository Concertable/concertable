using Core.Entities;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly IRegisterRepository registerRepository;
        private readonly IArtistService artistService;

        public RegisterService(IRegisterRepository registerRepository, IArtistService artistService)
        {
            this.registerRepository = registerRepository;
            this.artistService = artistService;
        }

        public async Task<IEnumerable<Register>> GetAllForListingIdAsync(int listingId)
        {
            return await registerRepository.GetAllForListingIdAsync(listingId);
        }

        public async Task RegisterForListing(int listingId)
        {
            var artist = await artistService.GetUserArtist();
            var listing = new Register()
            {
                ListingId = listingId,
                ArtistId = artist.Id,
                Approved = false
            };

            registerRepository.Add(listing);
        }
    }
}
