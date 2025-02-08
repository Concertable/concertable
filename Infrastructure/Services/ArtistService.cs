using AutoMapper;
using Core.Entities;
using Application.Interfaces;
using Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;

namespace Infrastructure.Services
{
    public class ArtistService : IArtistService
    {
        private readonly IArtistRepository artistRepository;
        private readonly IAuthService authService;
        private readonly IMapper mapper;

        public ArtistService(IAuthService authService, IArtistRepository artistRepository, IMapper mapper) 
        {
            this.artistRepository = artistRepository;
            this.authService = authService;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<ArtistHeaderDto>> GetHeadersAsync(SearchParams searchParams)
        {
            var headers = await artistRepository.GetHeadersAsync(searchParams);
            return mapper.Map<IEnumerable<ArtistHeaderDto>>(headers);
        }

        public async Task<ArtistDto?> GetDetailsForCurrentUserAsync()
        {
            var user = await authService.GetCurrentUserAsync();
            var artist = await artistRepository.GetByUserIdAsync(user.Id);

            return mapper.Map<ArtistDto?>(artist);
        }
    }
}
