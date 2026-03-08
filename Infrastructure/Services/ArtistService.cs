using Core.Entities;
using Application.Interfaces;
using Application.DTOs;
using Application.Mappers;
using Application.Requests;
using Core.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class ArtistService : IArtistService
{
    private readonly IArtistRepository artistRepository;
    private readonly IImageService imageService;
    private readonly IReviewService reviewService;
    private readonly ICurrentUserService currentUserService;
    private readonly IUnitOfWork unitOfWork;

    public ArtistService(
        IArtistRepository artistRepository,
        IImageService imageService,
        ICurrentUserService currentUserService,
        IReviewService reviewService,
        IUnitOfWork unitOfWork)
    {
        this.artistRepository = artistRepository;
        this.imageService = imageService;
        this.reviewService = reviewService;
        this.currentUserService = currentUserService;
        this.unitOfWork = unitOfWork;
    }

    public async Task<ArtistDto?> GetDetailsForCurrentUserAsync()
    {
        var user = await currentUserService.GetAsync();
        var artist = await artistRepository.GetByUserIdAsync(user.Id);
        return artist?.ToDto();
    }

    public async Task<ArtistDto> GetDetailsByIdAsync(int id)
    {
        var artist = await artistRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Artist not found");
        return artist.ToDto();
    }

    public async Task<ArtistDto> CreateAsync(CreateArtistRequest request, IFormFile image)
    {
        var artist = request.ToEntity();
        var user = await currentUserService.GetAsync();
        artist.UserId = user.Id;
        artist.ImageUrl = await imageService.UploadAsync(image);

        var createdArtist = await artistRepository.AddAsync(artist);
        await artistRepository.SaveChangesAsync();
        return createdArtist.ToDto();
    }

    public async Task<ArtistDto> UpdateAsync(ArtistDto artistDto, IFormFile? image)
    {
        var artist = await artistRepository.GetByIdAsync(artistDto.Id);
        if (artist is null)
            throw new NotFoundException("Artist not found");

        var user = await currentUserService.GetEntityAsync();
        if (artist.UserId != user.Id)
            throw new ForbiddenException("You do not own this Artist");

        artist.Name = artistDto.Name;
        artist.About = artistDto.About;
        artist.ImageUrl = artistDto.ImageUrl;

        var existingGenreIds = artist.ArtistGenres.Select(ag => ag.GenreId).ToList();
        var newGenreIds = artistDto.Genres.Select(g => g.Id).ToList();

        foreach (var genreId in existingGenreIds.Except(newGenreIds).ToList())
        {
            var toRemove = artist.ArtistGenres.FirstOrDefault(ag => ag.GenreId == genreId);
            if (toRemove != null)
                artist.ArtistGenres.Remove(toRemove);
        }

        foreach (var genreId in newGenreIds.Except(existingGenreIds).ToList())
        {
            artist.ArtistGenres.Add(new ArtistGenre { ArtistId = artist.Id, GenreId = genreId });
        }

        if (image is not null)
            artist.ImageUrl = await imageService.ReplaceAsync(image, artist.ImageUrl);

        artistRepository.Update(artist);
        await artistRepository.SaveChangesAsync();

        var result = artist.ToDto();
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
