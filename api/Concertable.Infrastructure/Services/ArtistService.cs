using Concertable.Core.Entities;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Application.DTOs;
using Concertable.Application.Mappers;
using Concertable.Application.Requests;
using Concertable.Application.Exceptions;
using Concertable.Infrastructure.Services.Geometry;
using Concertable.Identity.Contracts;
using Concertable.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Services;

public class ArtistService : IArtistService
{
    private readonly IArtistRepository artistRepository;
    private readonly IImageService imageService;
    private readonly ICurrentUser currentUser;
    private readonly ICurrentUserResolver currentUserResolver;
    private readonly IGeocodingService geocodingService;
    private readonly IGeometryProvider geometryProvider;
    private readonly IUnitOfWork unitOfWork;

    public ArtistService(
        IArtistRepository artistRepository,
        IImageService imageService,
        ICurrentUser currentUser,
        ICurrentUserResolver currentUserResolver,
        IGeocodingService geocodingService,
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider,
        IUnitOfWork unitOfWork)
    {
        this.artistRepository = artistRepository;
        this.imageService = imageService;
        this.currentUser = currentUser;
        this.currentUserResolver = currentUserResolver;
        this.geocodingService = geocodingService;
        this.geometryProvider = geometryProvider;
        this.unitOfWork = unitOfWork;
    }

    public async Task<ArtistDto> GetDetailsForCurrentUserAsync()
    {
        return await artistRepository.GetDtoByUserIdAsync(currentUser.GetId())
            ?? throw new NotFoundException("Artist not found");
    }

    public async Task<ArtistDto> GetDetailsByIdAsync(int id)
    {
        return await artistRepository.GetDtoByIdAsync(id)
            ?? throw new NotFoundException("Artist not found");
    }

    public async Task<ArtistDto> CreateAsync(CreateArtistRequest request)
    {
        var user = await currentUserResolver.ResolveAsync();

        var bannerUrl = await imageService.UploadAsync(request.Banner);
        var artist = ArtistEntity.Create(user.Id, request.Name, request.About, bannerUrl, request.Genres.Select(g => g.Id));

        var locationDto = await geocodingService.GetLocationAsync(request.Latitude, request.Longitude);
        artist.Location = geometryProvider.CreatePoint(request.Latitude, request.Longitude);
        artist.Address = new Address(locationDto.County, locationDto.Town);
        artist.Avatar = user.Avatar;
        artist.Email = user.Email;

        var createdArtist = await artistRepository.AddAsync(artist);
        await unitOfWork.SaveChangesAsync();

        return createdArtist.ToDto();
    }

    public async Task<ArtistDto> UpdateAsync(int id, UpdateArtistRequest request)
    {
        var artist = await artistRepository.GetFullByIdAsync(id)
            ?? throw new NotFoundException("Artist not found");

        if (artist.UserId != currentUser.GetId())
            throw new ForbiddenException("You do not own this Artist");

        var bannerUrl = request.Banner is not null
            ? await imageService.ReplaceAsync(request.Banner.File, request.Banner.Url)
            : artist.BannerUrl;

        artist.Update(request.Name, request.About, bannerUrl, request.Genres.Select(g => g.Id));

        var locationDto = await geocodingService.GetLocationAsync(request.Latitude, request.Longitude);
        artist.Location = geometryProvider.CreatePoint(request.Latitude, request.Longitude);
        artist.Address = new Address(locationDto.County, locationDto.Town);

        if (request.Avatar is not null)
            artist.Avatar = await imageService.ReplaceAsync(request.Avatar, artist.Avatar);

        await unitOfWork.SaveChangesAsync();

        return artist.ToDto();
    }

    public async Task<int> GetIdForCurrentUserAsync()
    {
        int? id = await artistRepository.GetIdByUserIdAsync(currentUser.GetId());

        if (id is null)
            throw new ForbiddenException("You do not own an Artist");

        return id.Value;
    }
}
