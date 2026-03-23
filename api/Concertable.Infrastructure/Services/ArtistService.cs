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
    private readonly ICurrentUser currentUser;
    private readonly IGenreSyncService genreSyncService;

    public ArtistService(
        IArtistRepository artistRepository,
        IImageService imageService,
        ICurrentUser currentUser,
        IGenreSyncService genreSyncService)
    {
        this.artistRepository = artistRepository;
        this.imageService = imageService;
        this.currentUser = currentUser;
        this.genreSyncService = genreSyncService;
    }

    public async Task<ArtistDto?> GetDetailsForCurrentUserAsync()
    {
        var user = currentUser.Get();
        var artist = await artistRepository.GetByUserIdAsync(user.Id);
        return artist?.ToDto();
    }

    public async Task<ArtistDto> GetDetailsByIdAsync(int id)
    {
        var artist = await artistRepository.GetDetailsByIdAsync(id)
            ?? throw new NotFoundException("Artist not found");
        return artist.ToDto();
    }

    public async Task<ArtistDto> CreateAsync(CreateArtistRequest request)
    {
        var artist = request.ToEntity();
        var user = currentUser.Get();
        artist.UserId = user.Id;
        artist.ImageUrl = await imageService.UploadAsync(request.Image);

        var createdArtist = await artistRepository.AddAsync(artist);
        await artistRepository.SaveChangesAsync();
        return createdArtist.ToDto();
    }

    public async Task<ArtistDto> UpdateAsync(int id, UpdateArtistRequest request)
    {
        var artist = await artistRepository.GetDetailsByIdAsync(id)
            ?? throw new NotFoundException("Artist not found");

        var user = currentUser.GetEntity();
        if (artist.UserId != user.Id)
            throw new ForbiddenException("You do not own this Artist");

        artist.Name = request.Name;
        artist.About = request.About;
        artist.ImageUrl = request.ImageUrl;

        genreSyncService.Sync(artist.ArtistGenres, request.Genres.Select(g => g.Id));

        if (request.Image is not null)
            artist.ImageUrl = await imageService.ReplaceAsync(request.Image, artist.ImageUrl);

        await artistRepository.SaveChangesAsync();

        return artist.ToDto();
    }

    public async Task<int> GetIdForCurrentUserAsync()
    {
        var user = currentUser.Get();
        int? id = await artistRepository.GetIdByUserIdAsync(user.Id);

        if (id is null)
            throw new ForbiddenException("You do not own an Artist");

        return id.Value;
    }

}
