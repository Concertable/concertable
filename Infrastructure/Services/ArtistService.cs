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
using Core.Responses;
using Infrastructure.Repositories;
using Core.Entities.Identity;

namespace Infrastructure.Services
{
    public class ArtistService : IArtistService
    {
        private readonly IArtistRepository artistRepository;
        private readonly IReviewService reviewService;
        private readonly ILocationService locationService;
        private readonly IAuthService authService;
        private readonly IMapper mapper;

        public ArtistService(
            IAuthService authService, 
            IReviewService reviewService, 
            ILocationService locationService,
            IArtistRepository artistRepository, 
            IMapper mapper) 
        {
            this.artistRepository = artistRepository;
            this.reviewService = reviewService;
            this.locationService = locationService;
            this.authService = authService;
            this.mapper = mapper;
        }

        public async Task<PaginationResponse<ArtistHeaderDto>> GetHeadersAsync(SearchParams searchParams)
        {
            var headers = await artistRepository.GetRawHeadersAsync(searchParams);

            await reviewService.AddAverageRatingsAsync(headers.Data);

            var locationHeaders = locationService.FilterAndSortByNearest(searchParams, headers.Data);

            return new PaginationResponse<ArtistHeaderDto>(
                locationHeaders,
                headers.TotalCount,
                headers.PageNumber,
                headers.PageSize);
        }

        public async Task<ArtistDto?> GetDetailsForCurrentUserAsync()
        {
            var user = await authService.GetCurrentUserAsync();
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

            var user = await authService.GetCurrentUserAsync();
            artist.UserId = user.Id;

            var createdArtist = await artistRepository.AddAsync(artist);
            await artistRepository.SaveChangesAsync();

            return mapper.Map<ArtistDto>(createdArtist);
        }
    }
}
