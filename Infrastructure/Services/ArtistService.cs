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
using Application.Responses;
using Infrastructure.Repositories;
using Core.Entities.Identity;

namespace Infrastructure.Services
{
    public class ArtistService : HeaderService<Artist, ArtistHeaderDto, IArtistRepository>, IArtistService
    {
        private readonly IArtistRepository artistRepository;
        private readonly IReviewService reviewService;
        private readonly ILocationService locationService;
        private readonly ICurrentUserService currentUserService;
        private readonly IMapper mapper;

        public ArtistService(
            IArtistRepository artistRepository,
            ICurrentUserService currentUserService, 
            IReviewService reviewService, 
            ILocationService locationService,
            IMapper mapper) : base(artistRepository, locationService)
        {
            this.artistRepository = artistRepository;
            this.reviewService = reviewService;
            this.locationService = locationService;
            this.currentUserService = currentUserService;
            this.mapper = mapper;
        }

        public async override Task<PaginationResponse<ArtistHeaderDto>> GetHeadersAsync(SearchParams searchParams)
        {
            var headers = await base.GetHeadersAsync(searchParams);

            await reviewService.AddAverageRatingsAsync(headers.Data);

            return headers;
        }

        public async Task<ArtistDto?> GetDetailsForCurrentUserAsync()
        {
            var user = await currentUserService.GetAsync();
            var artist = await artistRepository.GetByUserIdAsync(user.Id);

            return mapper.Map<ArtistDto?>(artist);
        }

        public async Task<ArtistDto> GetDetailsByIdAsync(int id)
        {
            var artist = await artistRepository.GetByIdAsync(id);
            return mapper.Map<ArtistDto>(artist);
        }

        public async Task<ArtistDto> CreateAsync(CreateArtistDto createArtistDto)
        {
            var artist = mapper.Map<Artist>(createArtistDto);

            var user = await currentUserService.GetAsync();
            artist.UserId = user.Id;

            var createdArtist = await artistRepository.AddAsync(artist);
            await artistRepository.SaveChangesAsync();

            return mapper.Map<ArtistDto>(createdArtist);
        }
    }
}
