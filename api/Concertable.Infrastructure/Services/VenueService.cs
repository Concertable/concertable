using Concertable.Core.Entities;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Infrastructure.Services.Geometry;
using Concertable.Application.Interfaces.Rating;
using Concertable.Application.DTOs;
using Concertable.Application.Mappers;
using Concertable.Application.Requests;
using Concertable.Core.Enums;
using Concertable.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Services;

public class VenueService : IVenueService
{
    private readonly IVenueRepository venueRepository;
    private readonly IUserRepository userRepository;
    private readonly IImageService imageService;
    private readonly IRatingRepository ratingRepository;
    private readonly ICurrentUser currentUser;
    private readonly IGeocodingService geocodingService;
    private readonly IGeometryProvider geometryService;
    private readonly IUnitOfWork unitOfWork;

    public VenueService(
        IVenueRepository venueRepository,
        IUserRepository userRepository,
        IImageService imageService,
        [FromKeyedServices(HeaderType.Venue)] IRatingRepository ratingRepository,
        ICurrentUser currentUser,
        IGeocodingService geocodingService,
        IUnitOfWork unitOfWork,
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryService)
    {
        this.venueRepository = venueRepository;
        this.userRepository = userRepository;
        this.imageService = imageService;
        this.ratingRepository = ratingRepository;
        this.currentUser = currentUser;
        this.geocodingService = geocodingService;
        this.unitOfWork = unitOfWork;
        this.geometryService = geometryService;
    }

    public async Task<VenueDto> GetDetailsByIdAsync(int id)
    {
        var venue = await venueRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Venue not found");
        var venueDto = venue.ToDto();
        venueDto.Rating = await ratingRepository.GetRatingAsync(venueDto.Id);
        return venueDto;
    }

    public async Task<VenueDto> CreateAsync(CreateVenueRequest request)
    {
        var venue = request.ToEntity();
        var user = currentUser.GetEntity();

        venue.UserId = user.Id;
        venue.BannerUrl = await imageService.UploadAsync(request.Banner);

        await UpdateUserLocationAsync(user, request.Latitude, request.Longitude);

        var createdVenue = await venueRepository.AddAsync(venue);
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync();

        return createdVenue.ToDto();
    }

    public async Task<VenueDto> UpdateAsync(int id, UpdateVenueRequest request)
    {
        var venue = await venueRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Venue not found");
        var user = currentUser.GetEntity();

        if (venue.UserId != user.Id)
            throw new ForbiddenException("You do not own this venue");

        venue.Name = request.Name;
        venue.About = request.About;
        venue.Approved = request.Approved;

        await UpdateUserLocationAsync(user, request.Latitude, request.Longitude);

        if (request.Banner is not null)
            venue.BannerUrl = await imageService.ReplaceAsync(request.Banner.File, request.Banner.Url);

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync();

        return venue.ToDto();
    }

    public async Task<VenueDto?> GetDetailsForCurrentUserAsync()
    {
        var user = currentUser.Get();
        var venue = await venueRepository.GetByUserIdAsync(user.Id);
        if (venue is null)
            return null;
        var venueDto = venue.ToDto();
        venueDto.Rating = await ratingRepository.GetRatingAsync(venueDto.Id);
        return venueDto;
    }

    public async Task<int> GetIdForCurrentUserAsync()
    {
        var user = currentUser.Get();
        int? id = await venueRepository.GetIdByUserIdAsync(user.Id);

        if (id is null)
            throw new ForbiddenException("You do not own a Venue");

        return id.Value;
    }

    public async Task ApproveAsync(int id)
    {
        var venue = await venueRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Venue not found");

        venue.Approved = true;
        await unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateUserLocationAsync(UserEntity user, double latitude, double longitude)
    {
        var location = await geocodingService.GetLocationAsync(latitude, longitude);
        user.County = location.County;
        user.Town = location.Town;
        user.Location = geometryService.CreatePoint(latitude, longitude);
    }
}
