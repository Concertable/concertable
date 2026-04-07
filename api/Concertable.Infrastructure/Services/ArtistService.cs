using Concertable.Core.Entities;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Infrastructure.Services.Geometry;
using Concertable.Application.DTOs;
using Concertable.Application.Mappers;
using Concertable.Application.Requests;
using Concertable.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Services;

public class ArtistService : IArtistService
{
    private readonly IArtistRepository artistRepository;
    private readonly IUserRepository userRepository;
    private readonly IImageService imageService;
    private readonly ICurrentUser currentUser;
    private readonly IGenreSyncService genreSyncService;
    private readonly IGeocodingService geocodingService;
    private readonly IGeometryProvider geometryService;
    private readonly IUnitOfWork unitOfWork;

    public ArtistService(
        IArtistRepository artistRepository,
        IUserRepository userRepository,
        IImageService imageService,
        ICurrentUser currentUser,
        IGenreSyncService genreSyncService,
        IGeocodingService geocodingService,
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryService,
        IUnitOfWork unitOfWork)
    {
        this.artistRepository = artistRepository;
        this.userRepository = userRepository;
        this.imageService = imageService;
        this.currentUser = currentUser;
        this.genreSyncService = genreSyncService;
        this.geocodingService = geocodingService;
        this.geometryService = geometryService;
        this.unitOfWork = unitOfWork;
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
        var user = currentUser.GetEntity();
        artist.UserId = user.Id;
        artist.BannerUrl = await imageService.UploadAsync(request.Banner);

        await UpdateUserLocationAsync(user, request.Latitude, request.Longitude);

        var createdArtist = await artistRepository.AddAsync(artist);
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync();

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

        genreSyncService.Sync(artist.ArtistGenres, request.Genres.Select(g => g.Id));

        await UpdateUserLocationAsync(user, request.Latitude, request.Longitude);

        if (request.Banner is not null)
            artist.BannerUrl = await imageService.ReplaceAsync(request.Banner.File, request.Banner.Url);

        if (request.Avatar is not null)
            user.Avatar = await imageService.ReplaceAsync(request.Avatar, user.Avatar);

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync();

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

    private async Task UpdateUserLocationAsync(UserEntity user, double latitude, double longitude)
    {
        var location = await geocodingService.GetLocationAsync(latitude, longitude);
        user.County = location.County;
        user.Town = location.Town;
        user.Location = geometryService.CreatePoint(latitude, longitude);
    }
}
