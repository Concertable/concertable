using Core.Entities;
using Core.Interfaces;
using Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ArtistService : IArtistService
    {
        private readonly IArtistRepository artistRepository;
        private readonly IAuthService authService;

        public ArtistService(IAuthService authService, IArtistRepository artistRepository) 
        {
            this.artistRepository = artistRepository;
            this.authService = authService;
        }

        public async Task<IEnumerable<Artist>> GetHeadersAsync(SearchParams searchParams)
        {
            return await artistRepository.GetHeadersAsync(searchParams);
        }

        public async Task<Artist?> GetUserArtist()
        {
            var user = authService.GetCurrentUser();
            return await artistRepository.GetByUserIdAsync(user.Id);
        }
    }
}
