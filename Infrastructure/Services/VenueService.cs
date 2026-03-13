using Core.Entities;
using Application.Interfaces;
using Application.DTOs;
using Application.Mappers;
using Application.Requests;
using Core.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class VenueService : IVenueService
{
    private readonly IVenueRepository venueRepository;
    private readonly IImageService imageService;
    private readonly IReviewService reviewService;
    private readonly ICurrentUser currentUser;
    private readonly IGeocodingService geocodingService;
    private readonly IGeometryProvider geometryService;
    private readonly IUnitOfWork unitOfWork;

    public VenueService(
        IVenueRepository venueRepository,
        IImageService imageService,
        IReviewService reviewService,
        ICurrentUser currentUser,
        IGeocodingService geocodingService,
        IUnitOfWork unitOfWork,
        IGeometryProvider geometryService)
    {
        this.venueRepository = venueRepository;
        this.imageService = imageService;
        this.reviewService = reviewService;
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
        await reviewService.SetAverageRatingAsync(venueDto);
        return venueDto;
    }

    public async Task<VenueDto> CreateAsync(CreateVenueRequest request)
    {
        var venueRepository = unitOfWork.GetRepository<Venue>();
        var userRepository = unitOfWork.GetBaseRepository<User>();

        var venue = request.ToEntity();
        var user = currentUser.GetEntity();

        venue.UserId = user.Id;
        venue.ImageUrl = await imageService.UploadAsync(request.Image);

        await UpdateUserLocationAsync(user, request.Latitude, request.Longitude);

        var createdVenue = await venueRepository.AddAsync(venue);
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync();

        return createdVenue.ToDto();
    }

    public async Task<VenueDto> UpdateAsync(int id, UpdateVenueRequest request)
    {
        var venueRepository = unitOfWork.GetRepository<Venue>();
        var userRepository = unitOfWork.GetBaseRepository<User>();

        var venue = await venueRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Venue not found");
        var user = currentUser.GetEntity();

        if (venue.UserId != user.Id)
            throw new ForbiddenException("You do not own this venue");

        venue.Name = request.Name;
        venue.About = request.About;
        venue.Approved = request.Approved;
        venue.ImageUrl = request.ImageUrl;

        await UpdateUserLocationAsync(user, request.Latitude, request.Longitude);

        if (request.Image is not null)
            venue.ImageUrl = await imageService.ReplaceAsync(request.Image);

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
        await reviewService.SetAverageRatingAsync(venueDto);
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

    private async Task UpdateUserLocationAsync(User user, double latitude, double longitude)
    {
        var location = await geocodingService.GetLocationAsync(latitude, longitude);
        user.County = location.County;
        user.Town = location.Town;
        user.Location = geometryService.CreatePoint(latitude, longitude);
    }
}
