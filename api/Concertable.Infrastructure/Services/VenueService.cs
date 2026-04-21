using Concertable.Core.Entities;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Application.DTOs;
using Concertable.Application.Mappers;
using Concertable.Application.Requests;
using Concertable.Shared.Exceptions;
using Concertable.Infrastructure.Services.Geometry;
using Concertable.Identity.Contracts;
using Concertable.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Services;

public class VenueService : IVenueService
{
    private readonly IVenueRepository venueRepository;
    private readonly IImageService imageService;
    private readonly ICurrentUser currentUser;
    private readonly IManagerModule managerModule;
    private readonly IGeocodingService geocodingService;
    private readonly IGeometryProvider geometryProvider;
    private readonly IUnitOfWork unitOfWork;

    public VenueService(
        IVenueRepository venueRepository,
        IImageService imageService,
        ICurrentUser currentUser,
        IManagerModule managerModule,
        IGeocodingService geocodingService,
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider,
        IUnitOfWork unitOfWork)
    {
        this.venueRepository = venueRepository;
        this.imageService = imageService;
        this.currentUser = currentUser;
        this.managerModule = managerModule;
        this.geocodingService = geocodingService;
        this.geometryProvider = geometryProvider;
        this.unitOfWork = unitOfWork;
    }

    public async Task<VenueDto> GetDetailsByIdAsync(int id)
    {
        return await venueRepository.GetDtoByIdAsync(id)
            ?? throw new NotFoundException("Venue not found");
    }

    public async Task<VenueDto> CreateAsync(CreateVenueRequest request)
    {
        var user = await managerModule.GetManagerAsync(currentUser.GetId())
            ?? throw new ForbiddenException("Manager not found");

        var bannerUrl = await imageService.UploadAsync(request.Banner);
        var venue = VenueEntity.Create(user.Id, request.Name, request.About, bannerUrl);

        var locationDto = await geocodingService.GetLocationAsync(request.Latitude, request.Longitude);
        venue.Location = geometryProvider.CreatePoint(request.Latitude, request.Longitude);
        venue.Address = new Address(locationDto.County, locationDto.Town);
        venue.Avatar = user.Avatar;
        venue.Email = user.Email;

        var createdVenue = await venueRepository.AddAsync(venue);
        await unitOfWork.SaveChangesAsync();

        return createdVenue.ToDto();
    }

    public async Task<VenueDto> UpdateAsync(int id, UpdateVenueRequest request)
    {
        var venue = await venueRepository.GetFullByIdAsync(id)
            ?? throw new NotFoundException("Venue not found");

        if (venue.UserId != currentUser.GetId())
            throw new ForbiddenException("You do not own this venue");

        var bannerUrl = request.Banner is not null
            ? await imageService.ReplaceAsync(request.Banner.File, request.Banner.Url)
            : venue.BannerUrl;

        venue.Update(request.Name, request.About, bannerUrl);

        var locationDto = await geocodingService.GetLocationAsync(request.Latitude, request.Longitude);
        venue.Location = geometryProvider.CreatePoint(request.Latitude, request.Longitude);
        venue.Address = new Address(locationDto.County, locationDto.Town);

        if (request.Avatar is not null)
            venue.Avatar = await imageService.ReplaceAsync(request.Avatar, venue.Avatar);

        await unitOfWork.SaveChangesAsync();

        return venue.ToDto();
    }

    public async Task<VenueDto> GetDetailsForCurrentUserAsync()
    {
        return await venueRepository.GetDtoByUserIdAsync(currentUser.GetId())
            ?? throw new NotFoundException("Venue not found");
    }

    public async Task<int> GetIdForCurrentUserAsync()
    {
        int? id = await venueRepository.GetIdByUserIdAsync(currentUser.GetId());

        if (id is null)
            throw new ForbiddenException("You do not own a Venue");

        return id.Value;
    }

    public async Task<bool> OwnsVenueAsync(int venueId)
    {
        var id = await venueRepository.GetIdByUserIdAsync(currentUser.GetId());
        return id == venueId;
    }

    public async Task ApproveAsync(int id)
    {
        var venue = await venueRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Venue not found");

        venue.Approve();
        await unitOfWork.SaveChangesAsync();
    }
}
