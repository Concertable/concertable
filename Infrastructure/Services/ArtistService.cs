using AutoMapper;
using Core.Entities;
using Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Core.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services
{
    public class ArtistService : IArtistService
    {
        private readonly IArtistRepository artistRepository;
        private readonly IImageService imageService;
        private readonly IReviewService reviewService;
        private readonly ICurrentUserService currentUserService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public ArtistService(
            IArtistRepository artistRepository,
            IImageService imageService,
            ICurrentUserService currentUserService,
            IReviewService reviewService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            this.artistRepository = artistRepository;
            this.imageService = imageService;
            this.reviewService = reviewService;
            this.currentUserService = currentUserService;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
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

        public async Task<ArtistDto> CreateAsync(CreateArtistDto createArtistDto, IFormFile image)
        {
            var artist = mapper.Map<Artist>(createArtistDto);
            var user = await currentUserService.GetAsync();
            artist.UserId = user.Id;
            artist.ImageUrl = await imageService.UploadAsync(image);

            foreach (var genre in createArtistDto.Genres)
            {
                artist.ArtistGenres.Add(new ArtistGenre
                {
                    ArtistId = artist.Id,
                    GenreId = genre.Id
                });
            }

            var createdArtist = await artistRepository.AddAsync(artist);
            await artistRepository.SaveChangesAsync();
            return mapper.Map<ArtistDto>(createdArtist);
        }

        public async Task<ArtistDto> UpdateAsync(ArtistDto artistDto, IFormFile? image)
        {
            var artist = await artistRepository.GetByIdAsync(artistDto.Id);
            if (artist is null)
                throw new NotFoundException("Artist not found");

            mapper.Map(artistDto, artist);

            var user = await currentUserService.GetEntityAsync();
            if (artist.UserId != user.Id)
                throw new ForbiddenException("You do not own this Artist");

            var existingGenreIds = artist.ArtistGenres.Select(ag => ag.GenreId).ToList();
            var newGenreIds = artistDto.Genres.Select(g => g.Id).ToList();

            var genreIdsToRemove = existingGenreIds.Except(newGenreIds).ToList();
            foreach (var genreId in genreIdsToRemove)
            {
                var genreToRemove = artist.ArtistGenres.FirstOrDefault(ag => ag.GenreId == genreId);
                if (genreToRemove != null)
                    artist.ArtistGenres.Remove(genreToRemove);
            }

            var genreIdsToAdd = newGenreIds.Except(existingGenreIds).ToList();
            foreach (var genreId in genreIdsToAdd)
            {
                artist.ArtistGenres.Add(new ArtistGenre
                {
                    ArtistId = artist.Id,
                    GenreId = genreId
                });
            }

            mapper.Map(artistDto, artist);

            if (image is not null)
                artist.ImageUrl = await imageService.ReplaceAsync(image, artist.ImageUrl);

            artistRepository.Update(artist);
            await artistRepository.SaveChangesAsync();

            var result = mapper.Map<ArtistDto>(artist);
            result.Genres = artistDto.Genres;
            return result;
        }

        public async Task<int> GetIdForCurrentUserAsync()
        {
            var user = await currentUserService.GetAsync();
            int? id = await artistRepository.GetIdByUserIdAsync(user.Id);

            if (id is null)
                throw new ForbiddenException("You do not own an Artist");

            return id.Value;
        }
    }
}
