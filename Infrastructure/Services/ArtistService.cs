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
using Core.Exceptions;

namespace Infrastructure.Services
{
    public class ArtistService : HeaderService<Artist, ArtistHeaderDto, IArtistRepository>, IArtistService
    {
        private readonly IArtistRepository artistRepository;
        private readonly IReviewService reviewService;
        private readonly ILocationService locationService;
        private readonly ICurrentUserService currentUserService;
        private IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public ArtistService(
            IArtistRepository artistRepository,
            ICurrentUserService currentUserService, 
            IReviewService reviewService, 
            IGeometryService geometryService,
            IUnitOfWork unitOfWork,
            IMapper mapper) : base(artistRepository, geometryService)
        {
            this.artistRepository = artistRepository;
            this.reviewService = reviewService;
            this.currentUserService = currentUserService;
            this.unitOfWork = unitOfWork;
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

        public async Task<ArtistDto> UpdateAsync(ArtistDto artistDto)
        {
            var artist = await artistRepository.GetByIdAsync(artistDto.Id);
            mapper.Map(artistDto, artist);

            var user = await currentUserService.GetEntityAsync();

            if (artist?.UserId != user.Id)
                throw new ForbiddenException("You do not own this Artist");

            var existingGenreIds = artist.ArtistGenres.Select(ag => ag.GenreId);

            var newGenreIds = artistDto.Genres.Select(g => g.Id);

            foreach (var genreId in existingGenreIds.Except(newGenreIds))
            {
                var genreToRemove = artist.ArtistGenres.First(ag => ag.GenreId == genreId);
                artist.ArtistGenres.Remove(genreToRemove);
            }

            foreach (var genreId in newGenreIds.Except(existingGenreIds))
            {
                artist.ArtistGenres.Add(new ArtistGenre
                {
                    ArtistId = artist.Id,
                    GenreId = genreId
                });
            }

            artistRepository.Update(artist);
            await artistRepository.SaveChangesAsync();

            return mapper.Map<ArtistDto>(artist);
        }
    }
}
